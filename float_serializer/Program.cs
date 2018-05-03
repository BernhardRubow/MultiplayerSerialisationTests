using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace float_serializer
{

    [Serializable]
    struct Vector3
    {
        public float x;
        public float y;
        public float z;
    }


    [Serializable]
    struct Quarternion
    {
        public float w;
        public float x;
        public float y;
        public float z;
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Serialize with Buffer.BlockCopy ...");
            testXtimes(10, () =>
            {
                long result = testDurationOf(useBufferBlockCopy);
                Console.WriteLine($"Result in ms: {result}");
            });

            Console.WriteLine("\n---\n");

            Console.WriteLine("Starting Serialize with BinaryFormatter ...");
            testXtimes(10, () =>
            {
                long result = testDurationOf(useBinaryFormatter);
                Console.WriteLine($"Result in ms: {result}");
            });
            Console.ReadLine();
        }

        /// <summary>
        /// Calls a given function delegate x times
        /// </summary>
        /// <param name="times">The number of times to call the function delegate.</param>
        /// <param name="actionToTest">The delegate to call.</param>
        public static void testXtimes(int times, Action actionToTest)
        {
            for (int i = 0; i < times; i++) actionToTest();
        }

        /// <summary>
        /// Measures the time for executing the given function delegate
        /// </summary>
        /// <param name="actionToTest">The function delegate to measure time for.</param>
        /// <returns>Runtime of function delegate in ms.</returns>
        public static long testDurationOf(Action<int, Vector3, Quarternion> actionToTest)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            sw.Start();

            actionToTest(100000, GetStandardVector(), GetRandomQuaternion());

            sw.Stop();

            return sw.ElapsedMilliseconds;
        }

        /// <summary>
        /// Serialize and Deserialize a Vector3 and a Quarternion for n-times by use
        /// of Buffer.BlockCopy.
        /// </summary>
        /// <param name="v">The vector to serialize</param>
        /// <param name="q">The quarternion to serialize</param>
        public static void useBufferBlockCopy(int n, Vector3 v, Quarternion q)
        {
            var FLOAT_SIZE = sizeof(float);

            var source = new [] {v.x, v.y, v.z, q.w, q.x, q.y, q.z};

            for (int i = 0; i < n; i++)
            {
                // create a byte array and copy the floats into it...
                var byteArray = new byte[source.Length*FLOAT_SIZE];
                Buffer.BlockCopy(source, 0, byteArray, 0, byteArray.Length);

                // create a second float array and copy the bytes into it...
                var floatArray2 = new float[byteArray.Length / 4];
                Buffer.BlockCopy(byteArray, 0, floatArray2, 0, byteArray.Length);

                Vector3 deserializedVector = new Vector3()
                {
                    x = floatArray2[0],
                    y = floatArray2[1],
                    z = floatArray2[2],
                };

                Quarternion deserializedQuaternion = new Quarternion
                {
                    w = floatArray2[3],
                    x = floatArray2[4],
                    y = floatArray2[5],
                    z = floatArray2[6]
                };
            }
        }

        /// <summary>
        /// Serialize and Deserialize a Vector3 and a Quarternion for n-times by use
        /// of MemoryStream and BinaryFormatter.
        /// </summary>
        /// <param name="v">The vector to serialize</param>
        /// <param name="q">The quarternion to serialize</param>
        public static void useBinaryFormatter(int n, Vector3 v3, Quarternion q4)
        {
            for (int i = 0; i < n; i++)
            {
                ArrayList list = new ArrayList() {v3, q4};

                // serialize ArrayList
                MemoryStream stream = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    formatter.Serialize(stream, list);
                }
                catch (SerializationException e)
                {
                    Console.WriteLine("Serialization Failed : " + e.Message);
                }
                byte[] objectAsBytes = stream.ToArray();
                stream.Close();
                
                // deserialize ArrayList
                stream = new MemoryStream();
                stream.Write(objectAsBytes, 0, objectAsBytes.Length);
                stream.Seek(0, SeekOrigin.Begin);
                formatter = new BinaryFormatter();
                try
                {
                    var objectThatWasDeserialized = formatter.Deserialize(stream);
                }
                catch (SerializationException e)
                {
                    Console.WriteLine("Deserialization Failed : " + e.Message);
                }
                stream.Close();
            }
        }


        // +++ helper +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        public static Vector3 GetRandomVector()
        {
            var v3 = new Vector3
            {
                x = GetRandomFloat(),
                y = GetRandomFloat(),
                z = GetRandomFloat()
            };
            return v3;
        }

        public static Vector3 GetStandardVector()
        {
            var v3 = new Vector3
            {
                x = 1f,
                y = 2f,
                z = 3f
            };
            return v3;
        }

        public static Quarternion GetRandomQuaternion()
        {
            var q4 = new Quarternion
            {
                w = GetRandomFloat(),
                x = GetRandomFloat(),
                y = GetRandomFloat(),
                z = GetRandomFloat()
            };
            return q4;
        }

        public static float GetRandomFloat()
        {
            var rnd = new Random();
            var mantissa = (rnd.NextDouble() * 2.0) - 1.0;
            var exponent = Math.Pow(2.0, rnd.Next(-126, 128));
            return (float)(mantissa * exponent);
        }
    }
}
