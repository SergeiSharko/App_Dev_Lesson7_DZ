using System.Reflection;
using System.Text;

namespace Lesson7_DZ
{
    //Разработайте атрибут позволяющий методу ObjectToString сохранять поля классов с использованием произвольного имени.
    //Метод StringToObject должен также уметь работать с этим атрибутом для записи значение в свойство по имени его атрибута.

    //[CustomName(“CustomFieldName”)]
    //public int I = 0.

    //Если использовать формат строки с данными использованной нами для предыдущего примера то пара ключ значение для свойства I выглядела бы CustomFieldName:0

    //Подсказка:Если GetProperty(propertyName) вернул null то очевидно свойства с таким именем нет и возможно имя является алиасом заданным с помощью CustomName.
    //Возможно, если перебрать все поля с таким атрибутом то для одного из них propertyName = совпадает с таковым заданным атрибутом.


    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class CustomNameAttribute : Attribute
    {
        public string Name { get; }
        public CustomNameAttribute(string name)
        {
            Name = name;
        }
    }

    public class Example
    {
        [CustomName("CustomFieldName")]
        public int I = 0;

        public string ObjectToString()
        {
            var fields = this.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var keyValuePairs = fields.Select(f =>
            {
                var attr = f.GetCustomAttribute<CustomNameAttribute>();
                var name = attr != null ? attr.Name : f.Name;
                var value = f.GetValue(this);
                return $"{name}:{value}";
            }).Concat(properties.Select(p =>
            {
                var attr = p.GetCustomAttribute<CustomNameAttribute>();
                var name = attr != null ? attr.Name : p.Name;
                var value = p.GetValue(this);
                return $"{name}:{value}";
            }));

            return string.Join(", ", keyValuePairs);
        }

        public void StringToObject(string data)
        {
            var keyValuePairs = data.Split(new[] { ", " }, StringSplitOptions.None)
                                    .Select(kv => kv.Split(':'))
                                    .ToDictionary(kv => kv[0], kv => kv[1]);

            var fields = this.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
            {
                var attr = field.GetCustomAttribute<CustomNameAttribute>();
                var name = attr != null ? attr.Name : field.Name;
                if (keyValuePairs.ContainsKey(name))
                {
                    var value = Convert.ChangeType(keyValuePairs[name], field.FieldType);
                    field.SetValue(this, value);
                }
            }

            foreach (var property in properties)
            {
                var attr = property.GetCustomAttribute<CustomNameAttribute>();
                var name = attr != null ? attr.Name : property.Name;
                if (keyValuePairs.ContainsKey(name))
                {
                    var value = Convert.ChangeType(keyValuePairs[name], property.PropertyType);
                    property.SetValue(this, value);
                }
            }
        }
    }

    public class Program
    {
        public static void Main()
        {
            var example = new Example();
            example.I = 42;
            string serialized = example.ObjectToString();
            Console.WriteLine(serialized);

            var newExample = new Example();
            newExample.StringToObject("CustomFieldName:100");
            Console.WriteLine(newExample.I); 
        }
    }
}
