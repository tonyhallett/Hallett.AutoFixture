using AutoFixture;
using AutoFixture.Kernel;

namespace Hallett.AutoFixture.Tests.Behaviour
{
    internal class DemoReplaceNodesTest
    {
        public class Requested
        {
            public bool Bool1 { get; set; }

            public bool Bool2 { get; set; }
        }

        public class ResolvedCapturingSpecimenBuilder<T>(ISpecimenBuilder inner) : ISpecimenBuilder
        {
            private readonly List<T> resolvedValues = [];

            public IReadOnlyList<T> ResolvedValues => resolvedValues;

            public object Create(object request, ISpecimenContext context)
            {
                var resolved = inner.Create(request, context);
                if (resolved is not NoSpecimen)
                {
                    resolvedValues.Add((T)resolved);
                }
                return resolved;
            }
        }

        [Test]
        public void Only_Bahaviors_Can_Change_The_Graph()
        {
            var fixture = new Fixture();
            ResolvedCapturingSpecimenBuilder<bool>? resolvedCapturingSpecimenBuilder = null;
            fixture.Behaviors.Add(new EngineTransformerBehaviour(engine =>
            {
                var sbs = new List<ISpecimenBuilder>();
                foreach (var sb in engine)
                {
                    if (sb is BooleanSwitch booleanSwitch)
                    {
                        resolvedCapturingSpecimenBuilder = new ResolvedCapturingSpecimenBuilder<bool>(booleanSwitch);
                        sbs.Add(resolvedCapturingSpecimenBuilder);
                    }
                    else
                    {
                        sbs.Add(sb);
                    }
                }
                return new CompositeSpecimenBuilder(sbs);
            }));

            var sut = fixture.Create<Requested>();
            var booleanSwitchResolvedValues = resolvedCapturingSpecimenBuilder!.ResolvedValues;


            Assert.Multiple(() =>
            {
                Assert.That(sut.Bool1, Is.Not.EqualTo(sut.Bool2));
                Assert.That(sut.Bool1, Is.EqualTo(booleanSwitchResolvedValues[0]));
                Assert.That(sut.Bool2, Is.EqualTo(booleanSwitchResolvedValues[1]));
            });
        }
    }

    class EngineTransformerBehaviour(Func<CompositeSpecimenBuilder, ISpecimenBuilder> engineTransform) : ISpecimenBuilderTransformation
    {
        public ISpecimenBuilderNode Transform(ISpecimenBuilder builder)
        {
            var node = (ISpecimenBuilderNode)builder;
            return node.ReplaceNodes(n =>
            {
                var autoPropertiesTarget = (AutoPropertiesTarget)n;
                var postProcessor = (Postprocessor)autoPropertiesTarget.Builder;
                var csb = (CompositeSpecimenBuilder)postProcessor.Builder;
                var multiple = csb.Skip(1).Single();
                var engine = (CompositeSpecimenBuilder)csb.First();
                return new AutoPropertiesTarget(
                    new Postprocessor(new CompositeSpecimenBuilder(engineTransform(engine), multiple), postProcessor.Command, postProcessor.Specification));
            }, n => n is AutoPropertiesTarget);
        }
    }

    public static class SpecimenBuilderNodeExtensions
    {
        public static ISpecimenBuilderNode ReplaceNodes(
            this ISpecimenBuilderNode graph,
            IEnumerable<ISpecimenBuilder> with,
            Func<ISpecimenBuilderNode, bool> when)
        {
            if (when(graph))
                return graph.Compose(with);

            var nodes = from b in graph
                        let n = b as ISpecimenBuilderNode
                        select n != null ? n.ReplaceNodes(with, when) : b;
            return graph.Compose(nodes);
        }

        public static ISpecimenBuilderNode ReplaceNodes(
            this ISpecimenBuilderNode graph,
            ISpecimenBuilderNode with,
            Func<ISpecimenBuilderNode, bool> when)
        {
            if (when(graph))
                return with;

            var nodes = from b in graph
                        let n = b as ISpecimenBuilderNode
                        select n != null ? n.ReplaceNodes(with, when) : b;
            return graph.Compose(nodes);
        }

        public static ISpecimenBuilderNode ReplaceNodes(
            this ISpecimenBuilderNode graph,
            Func<ISpecimenBuilderNode, ISpecimenBuilderNode> with,
            Func<ISpecimenBuilderNode, bool> when)
        {
            if (when(graph))
                return with(graph);

            var nodes = from b in graph
                        let n = b as ISpecimenBuilderNode
                        select n != null ? n.ReplaceNodes(with, when) : b;
            return graph.Compose(nodes);
        }

        /// <summary>
        /// Finds the first node in the passed graph that matches the specified predicate.
        /// If nothing is found - null is returned.
        /// </summary>
        public static ISpecimenBuilderNode? FindFirstNodeOrDefault(this ISpecimenBuilderNode graph, Func<ISpecimenBuilderNode, bool> predicate)
        {
            if (predicate.Invoke(graph))
                return graph;

            foreach (var builder in graph)
            {
                if (builder is ISpecimenBuilderNode builderNode)
                {
                    var result = FindFirstNodeOrDefault(builderNode, predicate);
                    if (result != null) return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the first node in the graph that matches the specified predicate.
        /// If no node is present - fails with exception.
        /// </summary>
        public static ISpecimenBuilderNode FindFirstNode(this ISpecimenBuilderNode graph, Func<ISpecimenBuilderNode, bool> predicate)
        {
            var result = graph.FindFirstNodeOrDefault(predicate);
            return result ?? throw new InvalidOperationException("Unable to find node matching the specified predicate.");
        }
    }
}
