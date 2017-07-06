using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ElectricAnalysis.DAL
{
    class SerializeData<T> where T : class
    {
        #region//Binary串行化
        /// <summary>
        /// 写入磁盘
        /// </summary>
        /// <param name="obj">实例对象</param>
        /// <param name="file">保存的文件路径跟文件名称</param>
        public static void L_WriteBinary(List<T> obj, string file)
        {
            using (Stream input = File.OpenWrite(file))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(input, obj);
            }
        }
        public static void T_WriteBinary(T obj, string file)
        {
            using (Stream input = File.OpenWrite(file))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(input, obj);
            }
        }

        /// <summary>
        /// 读出磁盘文件
        /// </summary>
        /// <param name="file">保存的文件路径跟文件名称</param>
        /// <returns>泛型集合对象</returns>
        public static List<T> L_ReadBinary(string file)
        {
            using (Stream outPut = File.OpenRead(file))
            {
                BinaryFormatter bf = new BinaryFormatter();
                object user = bf.Deserialize(outPut);
                if (user != null)
                {
                    return (user as List<T>);
                }
            }
            return null;
        }
        public static T T_ReadBinary(string file)
        {
            using (Stream outPut = File.OpenRead(file))
            {
                BinaryFormatter bf = new BinaryFormatter();
                object user = bf.Deserialize(outPut);
                if (user != null)
                {
                    return (user as T);
                }
            }
            return null;
        }
        /// <summary>
        /// 判断文件是否存在
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool fileexist(string file)
        {
            if (File.Exists(file))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 判断文件是否为空流
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool ifempty(string file)
        {
            if (File.OpenRead(file).Length == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        /// <summary>
        /// 单个对象转换成集合后进行串行化
        /// </summary>
        /// <param name="obj">单个对象</param>
        /// <returns>集合对象</returns>
        public static List<T> ConvertToCol(T obj)
        {
            List<T> col = new List<T>();
            col.Add(obj);
            return col;
        }
    }
}
