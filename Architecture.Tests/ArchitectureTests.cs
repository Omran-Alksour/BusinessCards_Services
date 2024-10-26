namespace Architecture.Tests
{

    public class ArchitectureTests
    {
        private const string DomainNamespace = "Domain";
        private const string ApplicationNamespace = "Application";
        private const string InfrastructureNamespace = "Infrastructure";
        private const string PresentationNamespace = "Presentation";
        private const string WebNamespace = "Web";

        [Fact]
        public void Domain_Should_Not_HaveDependencyOnOtherProjects()
        {
            //Arrange
            System.Reflection.Assembly assembly = typeof(Domain.AssemblyReference).Assembly;

            string[] otherProjects = new[]
            {
                ApplicationNamespace,
                InfrastructureNamespace,
                PresentationNamespace,
                WebNamespace
            };

            //Act
            TestResult testResult = Types.InAssembly(assembly).ShouldNot().HaveDependencyOnAny(otherProjects).GetResult();

            //Assert
            _ = testResult.IsSuccessful.Should().BeTrue();
        }


        [Fact]
        public void Application_Should_Not_HaveDependencyOnOtherProjects()
        {
            //Arrange
            System.Reflection.Assembly assembly = typeof(Application.AssemblyReference).Assembly;

            string[] otherProjects = new[]
            {
                InfrastructureNamespace,
                PresentationNamespace,
                WebNamespace
            };

            //Act
            TestResult testResult = Types.InAssembly(assembly).ShouldNot().HaveDependencyOnAny(otherProjects).GetResult();

            //Assert
            _ = testResult.IsSuccessful.Should().BeTrue();
        }


        [Fact]
        public void Handlers_Should_Have_DependencyOnDomain()
        {
            //Arrange
            System.Reflection.Assembly assembly = typeof(Application.AssemblyReference).Assembly;

            //Act
            TestResult testResult = Types.InAssembly(assembly).That().HaveNameEndingWith("Handler").Should().HaveDependencyOn(DomainNamespace).GetResult();

            //Assert
            _ = testResult.IsSuccessful.Should().BeTrue();
        }


        [Fact]
        public void Infrastructure_Should_Not_HaveDependencyOnOtherProjects()
        {
            //Arrange
            System.Reflection.Assembly assembly = typeof(Infrastructure.AssemblyReference).Assembly;

            string[] otherProjects = new[]
            {
                PresentationNamespace,
                WebNamespace
            };

            //Act
            TestResult testResult = Types.InAssembly(assembly).ShouldNot().HaveDependencyOnAny(otherProjects).GetResult();

            //Assert
            _ = testResult.IsSuccessful.Should().BeTrue();
        }


        [Fact]
        public void Presentation_Should_Not_HaveDependencyOnOtherProjects()
        {
            //Arrange
            System.Reflection.Assembly assembly = typeof(Presentation.AssemblyReference).Assembly;

            string[] otherProjects = new[]
            {
                InfrastructureNamespace,
                WebNamespace
            };

            //Act
            TestResult testResult = Types.InAssembly(assembly).ShouldNot().HaveDependencyOnAny(otherProjects).GetResult();

            //Assert
            _ = testResult.IsSuccessful.Should().BeTrue();
        }

    }

}