using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace AndWeHaveAPlan.Mimic
{
    public class Mimic<TInterface, TWorker> where TWorker : IProtocolImplementation
    {
        public Type Type { get; }

        public Mimic()
        {
            Type = MimicBuilder.Create<TInterface, TWorker>();
        }

        public TInterface NewInstance(TWorker implementation)
        {
            var obj = (TInterface) Activator.CreateInstance(Type, implementation);
            return obj;
        }
    }

    public static class MimicBuilder
    {
        //public static 

        private static void BuildCtor(ILGenerator ctorIlGen, FieldBuilder fieldBuilder)
        {
            /*
             public Something(TWorker worker){
                this._protocolImplementation = worker;
             }
            */
            ctorIlGen.Emit(OpCodes.Ldarg_0);
            ctorIlGen.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));
            ctorIlGen.Emit(OpCodes.Ldarg_0);
            ctorIlGen.Emit(OpCodes.Ldarg_1);
            ctorIlGen.Emit(OpCodes.Stfld, fieldBuilder);
            ctorIlGen.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TInterface">Interface to mimic to</typeparam>
        /// <typeparam name="TWorker">"Real" worker class</typeparam>
        /// <returns></returns>
        public static Type Create<TInterface, TWorker>() where TWorker : IProtocolImplementation
        {
            AssemblyName asmName = new AssemblyName
            {
                Name = "AndWeHaveAPlan.Mimic"
            };
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
            ModuleBuilder modBuild = assemblyBuilder.DefineDynamicModule("SampleModule");
            TypeBuilder tb = modBuild.DefineType("AndWeHaveAPlan.Mimic", TypeAttributes.Public);

            var fieldBuilder = tb.DefineField("_protocolImplementation", typeof(TWorker), FieldAttributes.Private);

            var ctorBuilder = tb.DefineConstructor(MethodAttributes.Public,
                CallingConventions.HasThis | CallingConventions.Standard, new[] {typeof(TWorker)});

            BuildCtor(ctorBuilder.GetILGenerator(), fieldBuilder);

            tb.AddInterfaceImplementation(typeof(TInterface));

            var interfaceMethods = typeof(TInterface).GetMethods();

            foreach (var interfaceMethodInfo in interfaceMethods)
            {
                string methodName = interfaceMethodInfo.Name;

                Type[] methodParamsTypes = interfaceMethodInfo.GetParameters().Select(pi => pi.ParameterType).ToArray();

                MethodBuilder methodBuilder =
                    tb.DefineMethod(
                        methodName, //название / имя метода 
                        MethodAttributes.Public |
                        MethodAttributes
                            .Virtual, // public virtual потому что сетоды реализующие нтерфейс на самом деле есть витруальные методы, как при наследовании
                        interfaceMethodInfo.ReturnType, //тип возвращаемого значения
                        methodParamsTypes // массив Type[] с типами параметров, которые принимает метод
                    );

                Console.WriteLine(interfaceMethodInfo.ReturnType.FullName);
                Console.WriteLine(interfaceMethodInfo.ReturnType.GUID);
                Console.WriteLine("////////////////////////////");

                var ilGenerator = methodBuilder.GetILGenerator();

                // this, private field
                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Ldfld, fieldBuilder);

                ilGenerator.Emit(OpCodes.Ldstr, methodName);
                ilGenerator.Emit(OpCodes.Ldc_I4, methodParamsTypes.Length);
                ilGenerator.Emit(OpCodes.Newarr, typeof(object));

                for (int i = 0; i < methodParamsTypes.Length; i++)
                {
                    ilGenerator.Emit(OpCodes.Dup); // дублируем указатель на массив
                    ilGenerator.Emit(OpCodes.Ldc_I4, i); //(a) кладем в стек индекс элемента в массиве Int32
                    ilGenerator.Emit(OpCodes.Ldarg, i + 1); //(b) кладем в стек сам аргумент метода
                    ilGenerator.Emit(OpCodes.Stelem_Ref); //кладем в массив (b) по индексу (a)
                }

                var methodInfo = typeof(TWorker).GetMethod("MakeRequest");


                // Task
                if (interfaceMethodInfo.ReturnType == typeof(Task))
                {
                    ilGenerator.EmitCall(OpCodes.Callvirt, methodInfo.MakeGenericMethod(typeof(object)),
                        new[] {typeof(string), typeof(object[])}); // (string address, object[] args)

                    // Task.CompletedTask
                 //   ilGenerator.Emit(OpCodes.Call,
                 //       typeof(Task).GetProperty("CompletedTask").GetMethod);
                }
                // Task<Something>
                else if (interfaceMethodInfo.ReturnType.BaseType == typeof(Task))
                {
                    var retType = interfaceMethodInfo.ReturnType.GenericTypeArguments.First();
                    ilGenerator.EmitCall(OpCodes.Callvirt, methodInfo.MakeGenericMethod(retType),
                        new[] {typeof(string), typeof(object[])}); // (string address, object[] args)
                }
                // other
                else
                {
                    ilGenerator.EmitCall(OpCodes.Callvirt, methodInfo.MakeGenericMethod(interfaceMethodInfo.ReturnType),
                        new[] {typeof(string), typeof(object[])});

                    ilGenerator.Emit(OpCodes.Call, typeof(Task<object>).GetProperty("Result").GetMethod);
                }


                // return;
                ilGenerator.Emit(OpCodes.Ret);

                MethodInfo newMethod = typeof(TInterface).GetMethod(methodName);

                tb.DefineMethodOverride(methodBuilder, newMethod);
            }

            // "билдим" тип, его можно положить в кэш
            var type = tb.CreateType();

            return type;

            //var obj = Activator.CreateInstance(type, Activator.CreateInstance(typeof(TWorker)));
            //var impl = (TInterface) obj;
            //return impl;
        }
    }
}