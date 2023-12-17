using System.Runtime.Serialization;
using AbsEngine.Rendering;

namespace AbsEngine.Exceptions
{
    public class GraphicsApiException : Exception
    {
        public GraphicsApiException()
        {
        }

        public GraphicsApiException(GraphicsAPIs expectedApi, GraphicsAPIs currentApi)
            : base($"Expected {expectedApi} while running in {currentApi}")
        {
            
        }

        public GraphicsApiException(string? message) : base(message)
        {
        }

        public GraphicsApiException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected GraphicsApiException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}