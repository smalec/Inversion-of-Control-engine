﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using IoC.Attributes;
using IoC.Exceptions;

namespace IoC
{
    public class SimpleContainer
    {
        public SimpleContainer()
        {
            _registeredInstances = new Dictionary<Type, object>();
        }

        public T Resolve<T>()
        {
            Type type = typeof(T);

            if (_registeredInstances.Keys.Contains(type))
                return (T) _registeredInstances[type];

            ConstructorInfo[] constructors = type.GetConstructors();
            ConstructorInfo dependencyConstructor = HaveDependencyConstructorAttribute(constructors);
            if (dependencyConstructor != null)
                return buildInstance<T>(dependencyConstructor, type);

            SortByParametersQuantity(constructors);
            for (int i = 0; i < constructors.Length; i++)
            {
                if (i + 1 < constructors.Length &&
                        constructors[i].GetParameters().Length == constructors[i + 1].GetParameters().Length)
                    throw new AmbiguousConstructorsException();
                try
                {
                    return buildInstance<T>(constructors[i], type);
                }
                catch (Exception)
                {
                    continue;
                }
            }

            throw new NotRegisteredImplementationException();
        }

        public void RegisterType<T>(bool singleton) where T : class
        {
            RegisterType<T, T>(singleton);
        }

        public void RegisterType<From, To>(bool singleton) where To : From
        {
            if (singleton)
                _registeredInstances.Add(typeof(From), Resolve<To>());
        }

        public void RegisterInstance<T>(T instance)
        {
            _registeredInstances.Add(typeof(T), instance);
        }

        private ConstructorInfo HaveDependencyConstructorAttribute(ConstructorInfo[] constructors)
        {
            ConstructorInfo result = null;
            foreach (ConstructorInfo constructor in constructors)
            {
                if (constructor.GetCustomAttributes(typeof(DependencyConstructor), true).Any())
                {
                    if (result != null)
                        throw new TooManyDependencyConstructorsAttrException();
                    result = constructor;
                }
            }
            return result;
        }

        private void SortByParametersQuantity(ConstructorInfo[] constructors)
        {
            Array.Sort(constructors, delegate(ConstructorInfo constructor1, ConstructorInfo constructor2)
            {
                return constructor2.GetParameters().Length.CompareTo(constructor1.GetParameters().Length);
            });
        }

        private object[] ResolveParameters(ConstructorInfo constructor)
        {
            object[] resolvedParameters = new object[constructor.GetParameters().Length];
            int i = 0;

            foreach (ParameterInfo parameter in constructor.GetParameters())
            {
                resolvedParameters[i] = typeof(SimpleContainer)
                                            .GetMethod("Resolve")
                                            .MakeGenericMethod(parameter.ParameterType)
                                            .Invoke(this, null);
                i++;
            }

            return resolvedParameters;
        }

        public T ResolveProperties<T>(T instance)
        {
            IEnumerable<PropertyInfo> properties = 
                        instance.GetType()
                                .GetProperties()
                                .Where(property => Attribute.IsDefined(property, typeof(DependencyProperty)));

            foreach (PropertyInfo property in properties)
            {
                property.SetValue(instance, typeof(SimpleContainer)
                                            .GetMethod("Resolve")
                                            .MakeGenericMethod(property.PropertyType)
                                            .Invoke(this, null));
            }

            return instance;
        }

        private T buildInstance<T>(ConstructorInfo constructor, Type type)
        {
            object[] parameters = ResolveParameters(constructor);
            T instance = (T) Activator.CreateInstance(type, parameters);
            return ResolveProperties<T>(instance);
        }

        private IDictionary<Type, object> _registeredInstances;
    }
}
