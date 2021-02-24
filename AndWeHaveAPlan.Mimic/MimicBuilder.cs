using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace AndWeHaveAPlan.Mimic
{
    /// <summary>
    /// 
    /// </summary>
    public static class MimicBuilder
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TInterface">Interface to mimic to</typeparam>
        /// <typeparam name="TMimicWorker">"Real" worker class</typeparam>
        /// <returns></returns>
        public static Type Create<TInterface, TMimicWorker>() where TMimicWorker : IMimicWorker
        {
            AssemblyName asmName = new AssemblyName
            {
                Name = "AndWeHaveAPlan.Mimic"
            };

            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndCollect);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("AndWeHaveAPlan.Mimic");

            /*
                public class TInterfaceMimic
                {
                    ...
             */
            TypeBuilder tb =
                moduleBuilder.DefineType($"AndWeHaveAPlan.{typeof(TInterface).Name}Mimic", TypeAttributes.Public);

            var makeRequestMethodInfo = typeof(TMimicWorker).GetMethod("DoWork");

            /*
                class ...
                {
                    private TMimicMock _protocolImplementation;
                    ...
                 }
             */
            var fieldBuilder = tb.DefineField(
                "_protocolImplementation",
                typeof(TMimicWorker),
                FieldAttributes.Private
            );

            /*
                public TInterfaceMimic(TWorker worker)
                {
                    this._protocolImplementation = worker;     
                }       
            */
            var ctorBuilder = tb.DefineConstructor(MethodAttributes.Public,
                CallingConventions.HasThis | CallingConventions.Standard, new[] { typeof(TMimicWorker) });
            BuildConstructor(ctorBuilder.GetILGenerator(), fieldBuilder);

            /*
                public class Mimic: TInterface
                {
                    ...
            */
            tb.AddInterfaceImplementation(typeof(TInterface));

            var interfaceMethods = typeof(TInterface).GetMethods();

            foreach (var interfaceMethodInfo in interfaceMethods)
            {
                var implementationMethodBuilder =
                    BuildImplementationMethod(interfaceMethodInfo, tb, fieldBuilder, makeRequestMethodInfo);



                MethodInfo newMethod = typeof(TInterface).GetMethod(interfaceMethodInfo.Name, interfaceMethodInfo.GetParameters().Select(p => p.ParameterType).ToArray());
                tb.DefineMethodOverride(implementationMethodBuilder, newMethod);
            }

            var type = tb.CreateType();

            return type;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interfaceMethodInfo">Mocking method</param>
        /// <param name="typeBuilder"></param>
        /// <param name="fieldBuilder"></param>
        /// <param name="mockMethodInfo"></param>
        /// <returns></returns>
        private static MethodBuilder BuildImplementationMethod(
            MethodInfo interfaceMethodInfo,
            TypeBuilder typeBuilder,
            FieldBuilder fieldBuilder,
            MethodInfo mockMethodInfo)
        {
            Type[] methodParamsTypes = interfaceMethodInfo.GetParameters().Select(pi => pi.ParameterType).ToArray();

            var methodBuilder = typeBuilder.DefineMethod(
                interfaceMethodInfo.Name,
                MethodAttributes.Public | MethodAttributes.Virtual, // this is interface method implementation
                interfaceMethodInfo.ReturnType,
                methodParamsTypes
            );

            var parameterConstructorInfo = typeof(MockParameter).GetConstructors()[0];

            string methodName = interfaceMethodInfo.Name;

            var ilGenerator = methodBuilder.GetILGenerator();

            /*
             stack state in comments
             for interface
             public interface IClient
             {
                 Task<int> Foo(string str);
             }
             */

            ilGenerator.Emit(OpCodes.Ldarg_0); // this
            ilGenerator.Emit(OpCodes.Ldfld, fieldBuilder); // this, _protocolImplementation

            // first argument
            ilGenerator.Emit(OpCodes.Ldstr, methodName); // _protocolImplementation, "Foo"

            ilGenerator.Emit(OpCodes.Ldc_I4, methodParamsTypes.Length); // _protocolImplementation, "Foo", 1
            ilGenerator.Emit(OpCodes.Newarr,
                typeof(MockParameter)); // _protocolImplementation, "Foo", *MockParameter[1]{}

            for (int i = 0; i < methodParamsTypes.Length; i++)
            {
                ilGenerator.Emit(OpCodes.Dup); // _protocolImplementation, "Foo", *MockParameter[1]{}, *MockParameter[1]
                ilGenerator.Emit(OpCodes.Ldc_I4,
                    i); // _protocolImplementation, "Foo", *MockParameter[1]{}, *MockParameter[1]{}, 0

                ilGenerator.Emit(OpCodes.Ldstr,
                    interfaceMethodInfo.GetParameters()[i]
                        .Name); // _protocolImplementation, "Foo", *MockParameter[1]{}, *MockParameter[1]{}, 0, "str"
                ilGenerator.Emit(OpCodes.Ldtoken,
                    methodParamsTypes[
                        i]); // _protocolImplementation, "Foo", *MockParameter[1]{}, *MockParameter[1]{}, 0, "str", string
                ilGenerator.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", new
                    Type[]
                    {
                        typeof(RuntimeTypeHandle)
                    })); // _protocolImplementation, "Foo", *MockParameter[1]{}, *MockParameter[1]{}, 0, "str", typeof(string)
                ilGenerator.Emit(OpCodes.Ldarg,
                    i + 1); // _protocolImplementation, "Foo", *MockParameter[1]{}, *MockParameter[1]{}, 0, "str", typeof(string), str
                if (methodParamsTypes[i].IsValueType)
                    ilGenerator.Emit(OpCodes.Box, methodParamsTypes[i]); // unbox for value types

                ilGenerator.Emit(OpCodes.Newobj,
                    parameterConstructorInfo); // _protocolImplementation, "Foo", *MockParameter[1]{}, *MockParameter[1]{}, MockParameter


                ilGenerator.Emit(OpCodes
                    .Stelem_Ref); // _protocolImplementation, "Foo", *MockParameter[1]{ MockParameter(s) }
            }


            // Task
            if (interfaceMethodInfo.ReturnType == typeof(Task))
            {
                /*
                    return _protocolImplementation.DoWork<object>(methodName, params[])   (Task<object>)
                 */
                ilGenerator.EmitCall(OpCodes.Callvirt, mockMethodInfo.MakeGenericMethod(typeof(object)),
                    new[] { typeof(string), typeof(MockParameter[]) }); // null

                ilGenerator.Emit(OpCodes.Ret);
            }
            // Task<Something>
            else if (interfaceMethodInfo.ReturnType.BaseType == typeof(Task))
            {
                /*
                    return _protocolImplementation.DoWork<TReturn>(methodName, params[])    (Task<TReturn>)
                 */
                var retType = interfaceMethodInfo.ReturnType.GenericTypeArguments.First();
                ilGenerator.EmitCall(OpCodes.Callvirt, mockMethodInfo.MakeGenericMethod(retType),
                    new[] { typeof(string), typeof(MockParameter[]) }); // Task<T>

                ilGenerator.Emit(OpCodes.Ret);
            }
            // other
            else
            {
                /*
                    return _protocolImplementation.DoWork<TReturn>(methodName, params[]).Result   (TReturn)
                 */
                ilGenerator.EmitCall(OpCodes.Callvirt,
                    mockMethodInfo.MakeGenericMethod(interfaceMethodInfo.ReturnType),
                    new[] { typeof(string), typeof(MockParameter[]) }); // this, Task<T>
                ilGenerator.Emit(OpCodes.Call, typeof(Task<object>).GetProperty("Result").GetMethod); // T

                ilGenerator.Emit(OpCodes.Ret);
            }

            return methodBuilder;
        }

        /// <summary>
        /// Build new type constructor
        /// </summary>
        /// <param name="ctorIlGen"></param>
        /// <param name="mockFieldBuilder"></param>
        private static void BuildConstructor(ILGenerator ctorIlGen, FieldBuilder mockFieldBuilder)
        {
            /*
                public Something(TWorker worker)
                {
                   this._protocolImplementation = worker;
                }
            */
            ctorIlGen.Emit(OpCodes.Ldarg_0);
            ctorIlGen.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));
            ctorIlGen.Emit(OpCodes.Ldarg_0);
            ctorIlGen.Emit(OpCodes.Ldarg_1);
            ctorIlGen.Emit(OpCodes.Stfld, mockFieldBuilder);
            ctorIlGen.Emit(OpCodes.Ret);
        }
    }
}