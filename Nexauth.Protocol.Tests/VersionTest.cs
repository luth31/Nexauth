using Xunit;
using Nexauth.Protocol;

namespace Nexauth.Protocol.Tests {
        public class VersionTest {

        [Fact]
        public void IsSupported_NoConditions_ReturnsTrue() {
            // Act
            bool supported = Version.IsSupported("0.1.0");
            // Assert
            Assert.True(supported);
        }
        
        [Fact]
        public void IsSupported_BadFormat_ReturnsFalse() {
            // Act
            bool supported = Version.IsSupported("a.1.0");
            // Assert
            Assert.False(supported);
        }

        [Fact]
        public void IsSupported_VersionOverflow_ReturnsFalse() {
            // Act
            bool supported = Version.IsSupported("10000000000000000000000000.1.0");
            // Assert
            Assert.False(supported);
        }
    }
}