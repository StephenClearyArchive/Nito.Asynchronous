using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

// This file contains all messages defined that are passed back and forth between the client and the server.
// For this test project, only two messages are defined.

namespace Messages
{
    /// <summary>
    /// Helper functions for message serialization and deserialization.
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// Serializes an object to a binary representation, returned as a byte array.
        /// </summary>
        /// <param name="Object">The object to serialize.</param>
        public static byte[] Serialize(object Object)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(stream, Object);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Deserializes an object from a binary representation.
        /// </summary>
        /// <param name="binaryObject">The byte array to deserialize.</param>
        public static object Deserialize(byte[] binaryObject)
        {
            using (MemoryStream stream = new MemoryStream(binaryObject))
            {
                return new BinaryFormatter().Deserialize(stream);
            }
        }
    }

    /// <summary>
    /// A message containing a single string.
    /// </summary>
    [Serializable]
    public class StringMessage
    {
        /// <summary>
        /// The string.
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// A message with more information.
    /// </summary>
    [Serializable]
    public class ComplexMessage
    {
        /// <summary>
        /// The user-defined string.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The time this message was created.
        /// </summary>
        public DateTimeOffset Time { get; set; }

        /// <summary>
        /// The unique identifier for this message.
        /// </summary>
        public Guid UniqueID { get; set; }
    }
}
