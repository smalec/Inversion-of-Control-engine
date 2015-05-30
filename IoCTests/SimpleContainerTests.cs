using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IoC.Attributes;
using IoC.Exceptions;

namespace IoC
{
    [TestClass]
    public class SimpleContainerTests
    {
        [TestMethod]
        public void ShouldRegisterNonSingleton()
        {
            SimpleContainer container = new SimpleContainer();
            container.RegisterType<Foo>(false);
            Foo f1 = container.Resolve<Foo>();
            Foo f2 = container.Resolve<Foo>();
            Assert.AreNotSame(f1, f2);
        }

        [TestMethod]
        public void ShouldRegisterSingleton()
        {
            SimpleContainer container = new SimpleContainer();
            container.RegisterType<Foo>(true);
            Foo f1 = container.Resolve<Foo>();
            Foo f2 = container.Resolve<Foo>();
            Assert.AreSame(f1, f2);
        }

        [TestMethod]
        public void ShouldRegisterInterfaceImplementation()
        {
            SimpleContainer container = new SimpleContainer();
            container.RegisterType<IBar, Bar>(true);
            Assert.IsInstanceOfType(container.Resolve<IBar>(), typeof(Bar));
        }

        [TestMethod]
        [ExpectedException(typeof(NotRegisteredImplementationException))]
        public void ShouldThrowExceptionForNotRegisteredInterfaceImplementation()
        {
            SimpleContainer container = new SimpleContainer();
            container.RegisterType<Bar>(false);
            container.Resolve<IBar>();
        }

        [TestMethod]
        public void ShouldRegisterInstance()
        {
            SimpleContainer container = new SimpleContainer();
            IBar bar = new Bar();
            container.RegisterInstance<IBar>(bar);
            Assert.AreSame(bar, container.Resolve<IBar>());
        }

        [TestMethod]
        public void ShouldResolveSingleConstructor()
        {
            SimpleContainer container = new SimpleContainer();
            A a = container.Resolve<A>();
            Assert.IsNotNull(a.b);
        }

        [TestMethod]
        public void ShouldResolveNotRegisteredTypes()
        {
            SimpleContainer container = new SimpleContainer();
            Assert.IsInstanceOfType(container.Resolve<Foo>(), typeof(Foo));
        }

        [TestMethod]
        public void ShouldResolveDependencyConstructorAttribute()
        {
            SimpleContainer container = new SimpleContainer();
            C c = container.Resolve<C>();
            Assert.IsNull(c.b);
        }

        [TestMethod]
        public void ShouldResolveMultipleConstructors()
        {
            SimpleContainer container = new SimpleContainer();
            D d = container.Resolve<D>();
            Assert.IsNotNull(d.b);
            Assert.IsNotNull(d.foo);
        }

        [TestMethod]
        [ExpectedException(typeof(NotRegisteredImplementationException))]
        public void ShouldNotResolveNoParameterStringConstructor()
        {
            SimpleContainer container = new SimpleContainer();
            X x = container.Resolve<X>();
        }

        [TestMethod]
        public void ShouldResolveRegisteredStringConstructor()
        {
            SimpleContainer container = new SimpleContainer();
            container.RegisterInstance<string>("Ala ma kota");
            X x = container.Resolve<X>();
            Assert.AreEqual("Ala ma kota", x.s);
        }
    }

    public class Foo { }

    public interface IBar { }

    public class Bar : IBar
    {
        public Bar()
        {

        }
    }

    public class A
    {
        public B b;

        public A(B b)
        {
            this.b = b;
        }
    }

    public class B { }

    public class C
    {
        public B b;

        [DependencyConstructor]
        public C()
        {
            this.b = null;
        }

        public C(B b)
        {
            this.b = b;
        }
    }

    public class D
    {
        public B b;
        public Foo foo;

        public D()
        {
            this.b = null;
            this.foo = null;
        }

        public D(B b)
        {
            this.b = b;
            this.foo = null;
        }

        public D(B b, Foo f)
        {
            this.b = b;
            this.foo = f;
        }
    }    public class X
    {
        public Y y;
        public string s;

        public X(Y y, string s)
        {
            this.y = y;
            this.s = s;
        }
    }

    public class Y { }
}
