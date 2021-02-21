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

            var makeRequestMethodInfo = typeof(TMimicWorker).GetMethod("Mock");

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
                CallingConventions.HasThis | CallingConventions.Standard, new[] {typeof(TMimicWorker)});
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

                MethodInfo newMethod = typeof(TInterface).GetMethod(interfaceMethodInfo.Name);
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

            // this
            ilGenerator.Emit(OpCodes.Ldarg_0);
            // private field
            ilGenerator.Emit(OpCodes.Ldfld, fieldBuilder);

            // first param "mockMethodName"
            ilGenerator.Emit(OpCodes.Ldstr, methodName);

            ilGenerator.Emit(OpCodes.Ldc_I4, methodParamsTypes.Length);
            ilGenerator.Emit(OpCodes.Newarr, typeof(MockParameter));

            for (int i = 0; i < methodParamsTypes.Length; i++)
            {
                ilGenerator.Emit(OpCodes.Dup); // dup array address
                ilGenerator.Emit(OpCodes.Ldc_I4, i); //load index

                ilGenerator.Emit(OpCodes.Ldstr, interfaceMethodInfo.GetParameters()[i].Name);
                ilGenerator.Emit(OpCodes.Ldtoken, methodParamsTypes[i]);
                ilGenerator.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", new
                    Type[] {typeof(RuntimeTypeHandle)}));
                ilGenerator.Emit(OpCodes.Ldarg, i + 1);
                if (methodParamsTypes[i].IsValueType)
                    ilGenerator.Emit(OpCodes.Box, methodParamsTypes[i]);

                ilGenerator.Emit(OpCodes.Newobj, parameterConstructorInfo);


                ilGenerator.Emit(OpCodes.Stelem_Ref);
            }


            // Task
            if (interfaceMethodInfo.ReturnType == typeof(Task))
            {
                /*
                    return _protocolImplementation.MakeRequest<object>(methodName, params[])   (Task<object>)
                 */
                ilGenerator.EmitCall(OpCodes.Callvirt, mockMethodInfo.MakeGenericMethod(typeof(object)),
                    new[] {typeof(string), typeof(MockParameter[])}); // (string address, object[] args)

                ilGenerator.Emit(OpCodes.Ret);
            }
            // Task<Something>
            else if (interfaceMethodInfo.ReturnType.BaseType == typeof(Task))
            {
                /*
                    return _protocolImplementation.MakeRequest<TReturn>(methodName, params[])    (Task<TReturn>)
                 */
                var retType = interfaceMethodInfo.ReturnType.GenericTypeArguments.First();
                ilGenerator.EmitCall(OpCodes.Callvirt, mockMethodInfo.MakeGenericMethod(retType),
                    new[] {typeof(string), typeof(MockParameter[])}); // (string address, object[] args)

                ilGenerator.Emit(OpCodes.Ret);
            }
            // other
            else
            {
                /*
                    return _protocolImplementation.MakeRequest<TReturn>(methodName, params[]).Result   (TReturn)
                 */
                ilGenerator.EmitCall(OpCodes.Callvirt,
                    mockMethodInfo.MakeGenericMethod(interfaceMethodInfo.ReturnType),
                    new[] {typeof(string), typeof(MockParameter[])});
                ilGenerator.Emit(OpCodes.Call, typeof(Task<object>).GetProperty("Result").GetMethod);

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