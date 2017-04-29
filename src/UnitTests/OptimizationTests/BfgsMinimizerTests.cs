﻿using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Optimization;
using NUnit.Framework;
using System.Text;
using MathNet.Numerics.UnitTests.OptimizationTests.TestFunctions;
using System.Collections.Generic;
using System.Collections;
using NUnit.Framework.Interfaces;

namespace MathNet.Numerics.UnitTests.OptimizationTests
{
    [TestFixture]
    public class BfgsMinimizerTests
    {
        [Test]
        public void FindMinimum_Rosenbrock_Easy()
        {
            var obj = ObjectiveFunction.Gradient(RosenbrockFunction.Value, RosenbrockFunction.Gradient);
            var solver = new BfgsMinimizer(1e-5, 1e-5, 1e-5, 1000);
            var result = solver.FindMinimum(obj, new DenseVector(new[] { 1.2, 1.2 }));

            Assert.That(Math.Abs(result.MinimizingPoint[0] - RosenbrockFunction.Minimum[0]), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - RosenbrockFunction.Minimum[1]), Is.LessThan(1e-3));
        }

        [Test]
        public void FindMinimum_Rosenbrock_Hard()
        {
            var obj = ObjectiveFunction.Gradient(RosenbrockFunction.Value, RosenbrockFunction.Gradient);
            var solver = new BfgsMinimizer(1e-5, 1e-5, 1e-5, 1000);
            var result = solver.FindMinimum(obj, new DenseVector(new[] { -1.2, 1.0 }));

            Assert.That(Math.Abs(result.MinimizingPoint[0] - RosenbrockFunction.Minimum[0]), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - RosenbrockFunction.Minimum[1]), Is.LessThan(1e-3));
        }

        [Test]
        public void FindMinimum_Rosenbrock_Overton()
        {
            var obj = ObjectiveFunction.Gradient(RosenbrockFunction.Value, RosenbrockFunction.Gradient);
            var solver = new BfgsMinimizer(1e-5, 1e-5, 1e-5, 1000);
            var result = solver.FindMinimum(obj, new DenseVector(new[] { -0.9, -0.5 }));

            Assert.That(Math.Abs(result.MinimizingPoint[0] - RosenbrockFunction.Minimum[0]), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - RosenbrockFunction.Minimum[1]), Is.LessThan(1e-3));
        }

        [Test]
        public void FindMinimum_BigRosenbrock_Easy()
        {
            var obj = ObjectiveFunction.Gradient(BigRosenbrockFunction.Value, BigRosenbrockFunction.Gradient);
            var solver = new BfgsMinimizer(1e-10, 1e-5, 1e-5, 1000);
            var result = solver.FindMinimum(obj, new DenseVector(new[] { 1.2*100.0, 1.2*100.0 }));

            Assert.That(Math.Abs(result.MinimizingPoint[0] - BigRosenbrockFunction.Minimum[0]), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - BigRosenbrockFunction.Minimum[1]), Is.LessThan(1e-3));
        }

        [Test]
        public void FindMinimum_BigRosenbrock_Hard()
        {
            var obj = ObjectiveFunction.Gradient(BigRosenbrockFunction.Value, BigRosenbrockFunction.Gradient);
            var solver = new BfgsMinimizer(1e-5, 1e-5, 1e-5, 1000);
            var result = solver.FindMinimum(obj, new DenseVector(new[] { -1.2*100.0, 1.0*100.0 }));

            Assert.That(Math.Abs(result.MinimizingPoint[0] - BigRosenbrockFunction.Minimum[0]), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - BigRosenbrockFunction.Minimum[1]), Is.LessThan(1e-3));
        }

        [Test]
        public void FindMinimum_BigRosenbrock_Overton()
        {
            var obj = ObjectiveFunction.Gradient(BigRosenbrockFunction.Value, BigRosenbrockFunction.Gradient);
            var solver = new BfgsMinimizer(1e-5, 1e-5, 1e-5, 1000);
            var result = solver.FindMinimum(obj, new DenseVector(new[] { -0.9*100.0, -0.5*100.0 }));

            Assert.That(Math.Abs(result.MinimizingPoint[0] - BigRosenbrockFunction.Minimum[0]), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - BigRosenbrockFunction.Minimum[1]), Is.LessThan(1e-3));
        }

        private class MghTestCaseEnumerator : IEnumerable<ITestCaseData>
        {
            public IEnumerator<ITestCaseData> GetEnumerator()
            {
                return
                    RosenbrockFunction2.TestCases
                        .Concat(BealeFunction.TestCases)
                        .Concat(HelicalValleyFunction.TestCases)
                        .Concat(MeyerFunction.TestCases)
                        .Concat(PowellSingularFunction.TestCases)
                        .Concat(WoodFunction.TestCases)
                        .Concat(BrownAndDennisFunction.TestCases)
                    .Where(x => x.IsUnbounded)
                    .Select<TestCase, ITestCaseData>(x => new TestCaseData(x)
                        .SetName(x.FullName)
                    )
                    .GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        [Test]
        [TestCaseSource(typeof(MghTestCaseEnumerator))]
        public void Mgh_Tests(TestFunctions.TestCase test_case)
        {
            var obj = new MghObjectiveFunction(test_case.Function, true, true);
            var solver = new BfgsMinimizer(1e-8, 1e-8, 1e-8, 1000);

            var result = solver.FindMinimum(obj, test_case.InitialGuess);

            if (test_case.MinimizingPoint != null)
            {
                Assert.That((result.MinimizingPoint - test_case.MinimizingPoint).L2Norm(), Is.LessThan(1e-3));
            }

            var val1 = result.FunctionInfoAtMinimum.Value;
            var val2 = test_case.MinimalValue;
            var abs_min = Math.Min(Math.Abs(val1), Math.Abs(val2));
            var abs_err = Math.Abs(val1 - val2);
            var rel_err = abs_err / abs_min;
            var success = (abs_min <= 1 && abs_err < 1e-3) || (abs_min > 1 && rel_err < 1e-3);
            Assert.That(success, "Minimal function value is not as expected.");
        }
    }
}