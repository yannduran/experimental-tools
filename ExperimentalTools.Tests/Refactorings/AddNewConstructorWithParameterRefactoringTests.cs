﻿using ExperimentalTools.Components;
using ExperimentalTools.Refactorings;
using ExperimentalTools.Tests.Infrastructure;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ExperimentalTools.Tests.Refactorings
{
    public class AddNewConstructorWithParameterRefactoringTests
    {
        [Theory, MemberData("HasActionTestData")]
        public async Task HasActionTest(string test, string input, string expectedOutput)
        {
            var acceptor = new CodeRefactoringActionAcceptor();
            var context = CodeRefactoringContextBuilder.Build(input, acceptor);

            var provider = new AddNewConstructorWithParameterRefactoring(new SimpleNameGenerator());
            await provider.ComputeRefactoringsAsync(context);

            Assert.True(acceptor.HasAction);

            var result = await acceptor.GetResultAsync(context);
            Assert.Equal(expectedOutput.HomogenizeLineEndings(), result.HomogenizeLineEndings());
        }

        public static IEnumerable<object[]> HasActionTestData =>
            new[]
            {
                new object[]
                {
                    "Constructor does not exist",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private readonly int @::@index;
    }
}",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private readonly int index;

        public TestService(int index)
        {
            this.index = index;
        }
    }
}"
                },
                new object[]
                {
                    "Constructor does not exist (at the end of declarator)",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private readonly int index@::@;
    }
}",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private readonly int index;

        public TestService(int index)
        {
            this.index = index;
        }
    }
}"
                },
                new object[]
                {
                    "Place new constructor below field group",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private int @::@index;
        private string name;

        public string Name { get { return name; } }

        private int i;
    }
}",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private int index;
        private string name;

        public TestService(int index)
        {
            this.index = index;
        }

        public string Name { get { return name; } }

        private int i;
    }
}"
                },
                new object[]
                {
                    "Place new constructor near existing constructors",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private int @::@index;
        private string name;

        public string Name { get { return name; } }

        public TestService(string name)
        {
            this.name = name;
        }

        private int i;
    }
}",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private int index;
        private string name;

        public string Name { get { return name; } }

        public TestService(string name)
        {
            this.name = name;
        }

        public TestService(int index)
        {
            this.index = index;
        }

        private int i;
    }
}"
                },
                new object[]
                {
                    "Field initialized from another parameterless constructor",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private int @::@index;

        public TestService()
        {
            this.index = 2;
        }
    }
}",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private int index;

        public TestService()
        {
            this.index = 2;
        }

        public TestService(int index)
        {
            this.index = index;
        }
    }
}"
                },
                new object[]
                {
                    "Field initialized from another constructor with other parameters",
                    @"
using System;

namespace HelloWorld
{
    class Test { public int Prop { get { return 2; } } }
    class TestService
    {
        private int @::@index;

        public TestService(Test test)
        {
            this.index = test.Prop;
        }
    }
}",
                    @"
using System;

namespace HelloWorld
{
    class Test { public int Prop { get { return 2; } } }
    class TestService
    {
        private int index;

        public TestService(Test test)
        {
            this.index = test.Prop;
        }

        public TestService(int index)
        {
            this.index = index;
        }
    }
}"
                },
                new object[]
                {
                    "Struct",
                    @"
using System;

namespace HelloWorld
{
    struct Test
    {
        private readonly int @::@index;
        public string name;
        public int count;
    }
}",
                    @"
using System;

namespace HelloWorld
{
    struct Test
    {
        private readonly int index;
        public string name;
        public int count;

        public Test(int index)
        {
            this.index = index;
            name = default(string);
            count = default(int);
        }
    }
}"
                },
                new object[]
                {
                    "Constructor with multipe paramters (but no assignment) exists",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private string name;
        private int @::@index;

        public TestService(string name, int index)
        {
            this.name = name;
        }
    }
}",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private string name;
        private int index;

        public TestService(string name, int index)
        {
            this.name = name;
        }

        public TestService(int index)
        {
            this.index = index;
        }
    }
}"
                },
                new object[]
                {
                    "Field already initialized (multiple constructors, but not any with a single vacant parameter of the same type)",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private int @::@m_index;
        private readonly string name;

        public TestService(string name)
        {
            this.name = name;
        }

        public TestService(string name, int index)
        {
            this.name = name;
            m_index = index;
        }
    }
}",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private int m_index;
        private readonly string name;

        public TestService(string name)
        {
            this.name = name;
        }

        public TestService(string name, int index)
        {
            this.name = name;
            m_index = index;
        }

        public TestService(int index)
        {
            m_index = index;
        }
    }
}"
                }
            };

        [Theory, MemberData("NoActionTestData")]
        public async Task NoActionTest(string test, string input)
        {
            var acceptor = new CodeRefactoringActionAcceptor();
            var context = CodeRefactoringContextBuilder.Build(input, acceptor);

            var provider = new AddNewConstructorWithParameterRefactoring(new SimpleNameGenerator());
            await provider.ComputeRefactoringsAsync(context);

            Assert.False(acceptor.HasAction);
        }

        public static IEnumerable<object[]> NoActionTestData =>
            new[]
            {
                new object[]
                {
                    "Outside field name",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private readonly i@::@nt index;
    }
}"
                },
                new object[]
                {
                    "Non-empty span",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private readonly int in@:d:@ex;
    }
}"
                },
                new object[]
                {
                    "Field already initialized (single constructor, single parameter)",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private int @::@m_index;
        public TestService(int index)
        {
            m_index = index;
        }
    }
}"
                },
                new object[]
                {
                    "Field already initialized (multiple constructors, including one with a single parameter)",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private int @::@m_index;
        private readonly string name;

        public TestService(string name)
        {
            this.name = name;
        }

        public TestService(string name, int index)
        {
            this.name = name;
            m_index = index;
        }

        public TestService(int index)
        {
            m_index = index;
        }
    }
}"
                },
                new object[]
                {
                    "A constructor with the single parameter of the same type as the field exists",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private int @::@m_index;
        public TestService(int index)
        {
        }
    }
}"
                },
                new object[]
                {
                    "Field is a constant",
                    @"
using System;

namespace HelloWorld
{
    class TestService
    {
        private const int @::@index = 1;
    }
}"
                }
            };
    }
}
