//using System;

using OQLWithReflectionDemo;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;

//var dataContext = CreateDataContext();

//var query = "/workorders/abc";
//var segments = query.Trim().Split('/')[1..];

//var contextType = typeof(DataContext);

//(Type type, object? obj) result;

//for (int i = 0; i < segments.Length; i++)
//{
//	var current = segments[i];
//	var next = (i + 1) < segments.Length ? segments[i + 1] : null;

//	var property = contextType.GetProperties().SingleOrDefault(p => p.Name.ToLower() == current.ToLower());

//	var isCollection = property != null && property.PropertyType.GetInterfaces().Contains(typeof(IEnumerable));
//	var isObject = property != null && !property.PropertyType.GetInterfaces().Contains(typeof(IEnumerable));
//	var isValue = !current.StartsWith("@") && !isCollection && !isObject;
//	var isFunction = current.StartsWith('#');

//	if (isCollection)
//	{
//		if (next is not null)
//		{

//		}
//		else
//		{
//			result = (property.PropertyType, property.GetValue(dataContext));
//		}
//	}

//	//Console.WriteLine($"isCollection: {isCollection}");
//	//Console.WriteLine($"isObject: {isObject}");
//	//Console.WriteLine($"isValue: {isValue}");
//	//Console.WriteLine($"isFunction: {isFunction}");
//}

var workOrder = new WorkOrder() 
{ 
	Number = "abc", 
	Brand = new Brand 
	{ 
		Name = "HP" 
	},
	Products = new List<Product> 
	{ 
		new Product { SerialNo = "CZ001" },
		new Product { SerialNo = "CZ002" },
		new Product { SerialNo = "CZ003" }
	}
};

var query = new string[] { "products", "CZ001" };

object? contextObject = workOrder;

foreach (var item in query)
{
	var property = contextObject?.GetType().GetProperties().SingleOrDefault(p => p.Name.ToLower() == item.Trim().ToLower());

	if (property is not null)
	{
		if (property.PropertyType.Namespace == "System")
			contextObject = property.GetValue(contextObject);
		//else if (property.PropertyType.GetInterfaces().Contains(typeof(IEnumerable)))
		//	contextObject = property.GetValue(contextObject);
		else
			contextObject = property.GetValue(contextObject);
	}
	else 
	{
		if (item.StartsWith("@"))
		{ 
			// handle function
		}

		if (contextObject?.GetType().GetInterfaces().Contains(typeof(IEnumerable)) ?? false)
		{
			foreach (var obj in (IEnumerable)contextObject)
			{
				var propertyWithAttribut = obj.GetType().GetProperties().SingleOrDefault(x => x.CustomAttributes.Any(x => x.AttributeType == typeof(GetAttribute)));

				if (propertyWithAttribut is not null)
					contextObject = obj;
			}
		}

	}
}

Console.WriteLine(contextObject.ToString());







DataContext CreateDataContext()
{
	return new DataContext
	{
		WorkOrders = new List<WorkOrder> 
		{
			new WorkOrder { Number = "abc" }
		}
	};
}

class DataContext
{
	public IEnumerable<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
}

public class WorkOrder
{
	public string Number { get; set; } = string.Empty;

	public Brand Brand { get; set; } = new Brand();

	public List<Product> Products { get; set; } = new List<Product>();
}

public class Product
{
	[GetAttribute()]
	public string SerialNo { get; set; } = string.Empty;
}

public class Brand
{
	public string Name { get; set; } = string.Empty;
}
