

using System.Collections;
using System.Reflection;

var @object = new TestClass 
{ 
	Nested = new TestClass { Number = 1000 },
	NestedCollection = new List<TestClass> 
	{ 
		new TestClass { Number = 1 },
		new TestClass { Number = 2 },
		new TestClass { Number = 3 },
		new TestClass { Number = 4 },
		new TestClass
		{
			Number = 5,
			Nested = new TestClass()
			{
				Number = 111,
				Nested = new TestClass()
				{
					Number = 222,
					Nested = new TestClass()
					{
						Number = 333
					}
				}
			}
		},
	}
};

var result = Read<int>(@object, "/nestedcollection/5/nested/nested/nested/number");

Console.ReadKey();


T? Read<T>(object? obj, string query)
{
	ArgumentNullException.ThrowIfNull(obj, nameof(obj));

	string[] parts = query.TrimStart('/').Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
	var firstPart = parts.FirstOrDefault();

	Type type = obj.GetType();

	if (type == typeof(int) ||
		type == typeof(string) ||
		type == typeof(bool) ||
		type == typeof(decimal) ||
		type == typeof(float) ||
		type == typeof(double))
	{
		return (T?)obj;
	}
	else if (ImplementsGenericInterface(obj, typeof(IDictionary<,>)))
	{
		if (firstPart is null)
		{
			return (T?)obj;
		}
		else
		{
			var value = obj.GetType()!.GetProperty("Item")!.GetValue(obj, new object[] { firstPart });
			return (T?)value;
		}
	}
	else if (ImplementsGenericInterface(obj, typeof(IEnumerable<>)))
	{
		if (firstPart is null)
		{
			return (T?)obj;
		}
		else
		{
			// /workorders/@property(workorder)/

			if (firstPart.StartsWith("@get("))
			{
				
			}
			else
			{
				var typeArgs = type.GetGenericArguments();
				var elementType = typeArgs[0];
				var properties = elementType.GetProperties();

				foreach (PropertyInfo propertyInfo in properties)
				{
					var attribute = propertyInfo.GetCustomAttributes(typeof(GetAttribute), true)
						.Cast<GetAttribute>()
						.SingleOrDefault();

					if (attribute is not null)
					{
						var firstOrDefaultItem = FirstOrDefault((IEnumerable)obj, propertyInfo, firstPart);
						return Read<T>(firstOrDefaultItem, string.Join("/", parts.Skip(1)));
					}
				}
			}
			
			return (T?)obj;
		}
	}
	else if (type.IsClass)
	{
		if (firstPart == null)
		{
			return (T?)obj;
		}
		else
		{
			var propertyInfo = obj.GetType().GetProperties().FirstOrDefault(p =>
				string.Equals(p.Name.ToLower(), firstPart.ToLower(), StringComparison.Ordinal));

			if (propertyInfo is null)
			{
				throw new Exception($"Property with name {firstPart} not found.");
			}

			return Read<T>(propertyInfo.GetValue(obj), string.Join("/", parts.Skip(1)));
		}
	}
	else
	{
		return default(T?);
	}
}

static bool ImplementsGenericInterface(object obj, Type interfaceType)
{
	Type objectType = obj.GetType();
	Type[] interfaces = objectType.GetInterfaces();

	foreach (Type i in interfaces)
	{
		if (i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType)
		{
			return true;
		}
	}
	return false;
}

object? FirstOrDefault(IEnumerable collection, PropertyInfo property, object value)
{
	// Získání enumeratoru pro kolekci
	IEnumerator enumerator = collection.GetEnumerator();

	while (enumerator.MoveNext())
	{
		// Získání hodnoty vlastnosti pro aktuální prvek
		var propertyValue = property.GetValue(enumerator.Current);

		try
		{
			// Převod hodnoty "value" na typ vlastnosti
			object convertedValue = Convert.ChangeType(value, property.PropertyType);

			// Porovnání hodnoty vlastnosti s hodnotou "convertedValue"
			if (propertyValue != null && propertyValue.Equals(convertedValue))
			{
				// Vrácení aktuálního prvku, pokud jeho vlastnost se shoduje s hodnotou "convertedValue"
				return enumerator.Current;
			}
		}
		catch (InvalidCastException)
		{
			// Ignorování chyby při převodu, pokud hodnota "value" není kompatibilní s typem vlastnosti
		}
	}

	// Vrácení null, pokud žádný prvek v kolekci nesplňuje podmínku
	return null;
}


public class TestClass
{
	[Get()]
	public int Number { get; set; } = 3;

	public TestClass? Nested { get; set; }

	public List<TestClass> NestedCollection { get; set; } = new List<TestClass>();
}


[AttributeUsage(AttributeTargets.Property)]
public class GetAttribute : Attribute
{
	public GetAttribute()
	{

	}
}
