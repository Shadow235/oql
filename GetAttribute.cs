namespace OQLWithReflectionDemo;

[System.AttributeUsage(System.AttributeTargets.Property)]
public class GetAttribute : Attribute
{
	public GetAttribute()
	{

	}
}
