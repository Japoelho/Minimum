using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Minimum.Proxy
{
    public class Proxies
    {
        private static Proxies _instance = null;
        private AssemblyBuilder _assembly;
        private ModuleBuilder _moduleBuilder;
        private IList<Type> _proxyTypes;

        private Proxies()
        {
            _proxyTypes = new List<Type>();

            //Assembly ass = Assembly.LoadFrom("MinimumDynamic.dll");

            AssemblyName assemblyName = new AssemblyName("MinimumDynamicAssembly");
            assemblyName.Version = new Version(1, 0, 0, 0);
            
            _assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
            _moduleBuilder = _assembly.DefineDynamicModule("DynamicModule", "MinimumDynamic.dll", true);
        }

        public static Proxies GetInstance()
        {
            if (_instance == null) { _instance = new Proxies(); }

            return _instance;
        }

        //public void SaveDynamic()
        //{
        //    _assembly.Save("MinimumDynamic.dll");
        //}

        public IProxy GetProxy(Type original)
        {
            if (!original.IsPublic) { throw new ArgumentException("The type to be proxied must be declared as public. Invalid type: " + original.Name); }

            Type proxy = _proxyTypes.FirstOrDefault(p => p.BaseType == original);
            if (proxy != null) { return (IProxy)Activator.CreateInstance(proxy); }

            string originalAssembly = original.Assembly.FullName.Substring(0, original.Assembly.FullName.IndexOf(','));
            TypeBuilder typeBuilder = _moduleBuilder.DefineType(originalAssembly + "." + original.Name + "Proxy", TypeAttributes.Public | TypeAttributes.Class, original, new Type[] { typeof(IProxy) });

            if (original.IsGenericType)
            {
                Type[] genericArguments = original.GetGenericArguments();
                string[] genericNames = new string[genericArguments.Length];
                
                for (int i = 0; i < genericArguments.Length; i++) { genericNames[i] = genericArguments[i].Name; }

                GenericTypeParameterBuilder[] genericParameterBuilder = typeBuilder.DefineGenericParameters(genericNames);
                typeBuilder.MakeGenericType(genericArguments);
            }

            foreach (CustomAttributeData attribute in original.CustomAttributes)
            {
                CustomAttributeBuilder attributeBuilder = CopyAttribute(attribute);
                typeBuilder.SetCustomAttribute(attributeBuilder);
            }

            foreach (PropertyInfo property in original.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(property.Name, property.Attributes, property.PropertyType, null);                
                foreach (CustomAttributeData attribute in property.CustomAttributes)
                {
                    CustomAttributeBuilder attributeBuilder = CopyAttribute(attribute);
                    propertyBuilder.SetCustomAttribute(attributeBuilder);
                }
            }
                        
            FieldBuilder _interceptor = ImplementIProxy(typeBuilder);

            MethodAttributes attributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Final;
            foreach (MethodInfo method in original.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (method.IsVirtual == false) { continue; }

                ParameterInfo[] parametersInfo = method.GetParameters();
                Type[] parameters = new Type[parametersInfo.Length];
                for (int p = 0; p < parametersInfo.Length; p++)
                { parameters[p] = parametersInfo[p].ParameterType; }

                MethodBuilder overrideMethod = typeBuilder.DefineMethod(method.Name, attributes, method.ReturnType, parameters);
                ILGenerator getIL = overrideMethod.GetILGenerator();

                foreach (CustomAttributeData attribute in method.GetCustomAttributesData())
                {
                    overrideMethod.SetCustomAttribute(CopyAttribute(attribute));
                }

                if (method.ReturnType != typeof(void))
                {
                    LocalBuilder args = getIL.DeclareLocal(typeof(Object[]));
                    LocalBuilder result = getIL.DeclareLocal(typeof(Object));
                    
                    // - Intercept Before
                    getIL.Emit(OpCodes.Ldarg_0);
                    getIL.Emit(OpCodes.Ldfld, _interceptor);
                    getIL.Emit(OpCodes.Ldstr, method.Name);
                    getIL.Emit(OpCodes.Ldarg_0);
                    getIL.Emit(OpCodes.Box, typeBuilder.BaseType);
                    getIL.Emit(OpCodes.Ldc_I4, parameters.Length);
                    getIL.Emit(OpCodes.Newarr, typeof(Object));

                    getIL.Emit(OpCodes.Stloc_0);
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        getIL.Emit(OpCodes.Ldloc_0);
                        getIL.Emit(OpCodes.Ldc_I4, i);
                        getIL.Emit(OpCodes.Ldarg_S, i + 1);
                        getIL.Emit(OpCodes.Box, parametersInfo[i].ParameterType);
                        getIL.Emit(OpCodes.Stelem_Ref);
                    }
                    getIL.Emit(OpCodes.Ldloc_0);

                    getIL.Emit(OpCodes.Callvirt, typeof(Interceptor).GetMethod("InterceptBefore", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
                    
                    // - Call the Original Method
                    getIL.Emit(OpCodes.Ldarg_0);
                    for (int i = 1; i <= parameters.Length; i++)
                    { getIL.Emit(OpCodes.Ldarg_S, i); }
                    getIL.Emit(OpCodes.Call, method);
                    getIL.Emit(OpCodes.Box, method.ReturnType);
                    getIL.Emit(OpCodes.Stloc_1);

                    // - Intercept After
                    getIL.Emit(OpCodes.Ldarg_0);
                    getIL.Emit(OpCodes.Ldfld, _interceptor);                    
                    getIL.Emit(OpCodes.Ldstr, method.Name);
                    getIL.Emit(OpCodes.Ldarg_0);
                    getIL.Emit(OpCodes.Box, typeBuilder.BaseType);
                    getIL.Emit(OpCodes.Ldloc_0);
                    getIL.Emit(OpCodes.Ldloca, 1);
                    getIL.Emit(OpCodes.Callvirt, typeof(Interceptor).GetMethod("InterceptAfter", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));

                    // - Return the result
                    getIL.Emit(OpCodes.Ldloc_1);
                    getIL.Emit(OpCodes.Unbox_Any, method.ReturnType);
                }
                else
                {
                    getIL.Emit(OpCodes.Ldarg_0);
                    for (int i = 1; i <= parameters.Length; i++)
                    { getIL.Emit(OpCodes.Ldarg_S, i); }
                    getIL.Emit(OpCodes.Call, method);
                }
                getIL.Emit(OpCodes.Ret);

                typeBuilder.DefineMethodOverride(overrideMethod, method);
            }

            proxy = typeBuilder.CreateType();
            _proxyTypes.Add(proxy);
            
            return (IProxy)Activator.CreateInstance(proxy);
        }

        private FieldBuilder ImplementIProxy(TypeBuilder typeBuilder)
        {
            // - Private Field
            FieldBuilder _interceptor = typeBuilder.DefineField("_interceptor", typeof(Interceptor), FieldAttributes.Private);
            FieldBuilder _original = typeBuilder.DefineField("_original", typeof(Type), FieldAttributes.Private);

            // - Property Accessor
            PropertyInfo interceptorProperty = typeof(IProxy).GetProperty("Interceptor");
            PropertyInfo originalProperty = typeof(IProxy).GetProperty("Original");

            MethodAttributes attributes = MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot;

            // - Get
            MethodInfo getInterceptor = interceptorProperty.GetGetMethod();
            MethodBuilder getInterceptorBuilder = typeBuilder.DefineMethod(getInterceptor.Name, MethodAttributes.Public | attributes, typeof(Interceptor), Type.EmptyTypes);
            ILGenerator getIIL = getInterceptorBuilder.GetILGenerator();
            getIIL.Emit(OpCodes.Ldarg_0);
            getIIL.Emit(OpCodes.Ldfld, _interceptor);
            getIIL.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(getInterceptorBuilder, getInterceptor);

            MethodInfo getOriginal = originalProperty.GetGetMethod();
            MethodBuilder getOriginalBuilder = typeBuilder.DefineMethod(getOriginal.Name, MethodAttributes.Public | attributes, typeof(Type), Type.EmptyTypes);
            ILGenerator getOIL = getOriginalBuilder.GetILGenerator();
            getOIL.Emit(OpCodes.Ldarg_0);
            getOIL.Emit(OpCodes.Ldfld, _original);
            getOIL.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(getOriginalBuilder, getOriginal);

            // - Set
            //MethodInfo setProperty = property.GetSetMethod();
            //MethodBuilder setInterceptor = typeBuilder.DefineMethod(setProperty.Name, MethodAttributes.Private | attributes, typeof(void), new Type[] { typeof(Interceptor) });
            //ILGenerator setIL = setInterceptor.GetILGenerator();
            //setIL.Emit(OpCodes.Ret);

            //typeBuilder.DefineMethodOverride(setInterceptor, setProperty);

            // - Constructors
            ConstructorInfo _interceptorConstructor = typeof(Interceptor).GetConstructor(Type.EmptyTypes);
            foreach (ConstructorInfo constructorInfo in typeBuilder.BaseType.GetConstructors())
            {
                // - Parameters
                ParameterInfo[] parametersInfo = constructorInfo.GetParameters();
                Type[] parameters = new Type[parametersInfo.Length];
                for (int p = 0; p < parametersInfo.Length; p++)
                { parameters[p] = parametersInfo[p].ParameterType; }

                ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig, CallingConventions.Standard, parameters);
                ILGenerator ilGen = constructorBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Newobj, _interceptorConstructor);
                ilGen.Emit(OpCodes.Stfld, _interceptor);

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Call, typeof(Object).GetMethod("GetType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
                ilGen.Emit(OpCodes.Callvirt, typeof(Type).GetMethod("get_BaseType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
                ilGen.Emit(OpCodes.Stfld, _original);

                ilGen.Emit(OpCodes.Ldarg_0);
                for (int i = 1; i <= parameters.Length; i++)
                { ilGen.Emit(OpCodes.Ldarg_S, i); }
                ilGen.Emit(OpCodes.Call, constructorInfo);
                ilGen.Emit(OpCodes.Ret);
            }

            return _interceptor;
        }

        private CustomAttributeBuilder CopyAttribute(CustomAttributeData attrData)
        {
            if (attrData.NamedArguments == null)
            {
                CustomAttributeBuilder attrBuilder = new CustomAttributeBuilder(
                  attrData.Constructor,
                  attrData.ConstructorArguments
                          .Select(ca => ca.Value)
                          .ToArray()
                  );
                return attrBuilder;
            }
            else
            {
                CustomAttributeBuilder attrBuilder = new CustomAttributeBuilder(
                  attrData.Constructor,
                  attrData.ConstructorArguments
                          .Select(ca => ca.Value)
                          .ToArray(),
                  attrData.NamedArguments
                          .Where(na => na.MemberInfo is PropertyInfo)
                          .Select(na => na.MemberInfo as PropertyInfo)
                          .ToArray(),
                  attrData.NamedArguments
                          .Where(na => na.MemberInfo is PropertyInfo)
                          .Select(na => na.TypedValue.Value)
                          .ToArray(),
                  attrData.NamedArguments
                          .Where(na => na.MemberInfo is FieldInfo)
                          .Select(na => na.MemberInfo as FieldInfo)
                          .ToArray(),
                  attrData.NamedArguments
                          .Where(na => na.MemberInfo is FieldInfo)
                          .Select(na => na.TypedValue.Value)
                          .ToArray()
                  );
                return attrBuilder;
            }
        }
    }
}