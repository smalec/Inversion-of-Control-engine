using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoC.Attributes
{
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false)]
    public class DependencyConstructor : Attribute
    {
    }
}
