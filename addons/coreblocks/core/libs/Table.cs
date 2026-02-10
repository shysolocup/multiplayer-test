using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;


[GlobalClass]
public partial class Table : Singleton<Table>
{
    public static TValue[] Unpack<[MustBeVariant] TValue>(Array<TValue> array)
        => array.ToArray();

    public static Variant Pack(params Variant[] array)
        => new Array<Variant>(array);

    public static (TValue, int)[] IPairs<[MustBeVariant] TValue>(GodotObject self, Array<TValue> arr)
		=> [.. arr.ToArray().Select((value, i) => (value, i))];

	public static (TKey, TValue)[] Pairs<[MustBeVariant] TKey, [MustBeVariant] TValue>(GodotObject self, Godot.Collections.Dictionary<TKey, TValue> dict)
	{
		(TKey, TValue)[] array = [];

		foreach (var pair in dict)
		{
			array.Append(
				(pair.Key, pair.Value)
			);
		}

		return array;
	}

	public static (Variant, Variant)[] Pairs(GodotObject self, Godot.Collections.Dictionary dict)
	{
		(Variant, Variant)[] array = [];

		foreach (var pair in dict)
		{
			array.Append(
				(pair.Key, pair.Value)
			);
		}

		return array;
	}
}