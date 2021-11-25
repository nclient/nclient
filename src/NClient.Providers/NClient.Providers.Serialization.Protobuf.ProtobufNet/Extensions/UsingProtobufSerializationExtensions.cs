using NClient.Common.Helpers;
using NClient.Providers.Serialization;
using NClient.Providers.Serialization.Protobuf.ProtobufNet;

// ReSharper disable once CheckNamespace
namespace NClient
{
    public static class UsingProtobufSerializationExtensions
    {
        /// <summary>
        /// Sets ProtobufNet based <see cref="ISerializerProvider"/> used to create instance of <see cref="ISerializer"/>.
        /// </summary>
        /// <param name="serializationBuilder"></param>
        public static INClientOptionalBuilder<TClient, TRequest, TResponse> UsingProtobufNetSerialization<TClient, TRequest, TResponse>(
            this INClientSerializationBuilder<TClient, TRequest, TResponse> serializationBuilder)
            where TClient : class
        {
            Ensure.IsNotNull(serializationBuilder, nameof(serializationBuilder));

            return serializationBuilder.UsingCustomSerializer(new ProtobufSerializerProvider());
        }

        /// <summary>
        /// Sets ProtobufNet based <see cref="ISerializerProvider"/> used to create instance of <see cref="ISerializer"/>.
        /// </summary>
        /// <param name="serializationBuilder"></param>
        /// <param name="protobufSerializerSettings">The settings to be used with <see cref="ProtobufSerializer"/>.</param>
        public static INClientOptionalBuilder<TClient, TRequest, TResponse> UsingProtobufNetSerialization<TClient, TRequest, TResponse>(
            this INClientSerializationBuilder<TClient, TRequest, TResponse> serializationBuilder,
            ProtobufSerializerSettings protobufSerializerSettings)
            where TClient : class
        {
            Ensure.IsNotNull(serializationBuilder, nameof(serializationBuilder));
            Ensure.IsNotNull(protobufSerializerSettings, nameof(protobufSerializerSettings));

            return serializationBuilder
                .UsingCustomSerializer(new ProtobufSerializerProvider(protobufSerializerSettings));
        }

        /// <summary>
        /// Sets ProtobufNet based <see cref="ISerializerProvider"/> used to create instance of <see cref="ISerializer"/>.
        /// </summary>
        /// <param name="serializationBuilder"></param>
        public static INClientFactoryOptionalBuilder<TRequest, TResponse> UsingProtobufNetSerialization<TRequest, TResponse>(
            this INClientFactorySerializationBuilder<TRequest, TResponse> serializationBuilder)
        {
            Ensure.IsNotNull(serializationBuilder, nameof(serializationBuilder));

            return serializationBuilder.UsingCustomSerializer(new ProtobufSerializerProvider());
        }

        /// <summary>
        /// Sets ProtobufNet based <see cref="ISerializerProvider"/> used to create instance of <see cref="ISerializer"/>.
        /// </summary>
        /// <param name="serializationBuilder"></param>
        /// <param name="protobufSerializerSettings">The settings to be used with <see cref="ProtobufSerializer"/>.</param>
        public static INClientFactoryOptionalBuilder<TRequest, TResponse> UsingProtobufNetSerialization<TRequest, TResponse>(
            this INClientFactorySerializationBuilder<TRequest, TResponse> serializationBuilder,
            ProtobufSerializerSettings protobufSerializerSettings)
        {
            Ensure.IsNotNull(serializationBuilder, nameof(serializationBuilder));
            Ensure.IsNotNull(protobufSerializerSettings, nameof(protobufSerializerSettings));

            return serializationBuilder
                .UsingCustomSerializer(new ProtobufSerializerProvider(protobufSerializerSettings));
        }
    }
}
