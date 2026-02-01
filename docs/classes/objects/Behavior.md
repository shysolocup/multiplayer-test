
---

{% include-markdown "../../types/classdef.md" start="<!--class_head-->" end="<!--class_head_end-->" %}

# Behavior
##### Inherits from <code><a href="https://docs.godotengine.org/en/stable/classes/class_node.html">Node</a></code>, <code><a href="../../singletons/IBaseSingleton">IBaseSingleton</a></code> ➜ <code><a href="../../singletons/Singleton">Singleton</a></code>

{% include-markdown "../../types/classdef.md" start="<!--rep-->" end="<!--rep_end-->" %}

Physical script node with a new easier to interact with interface.

```cs
public partial class GrunkTinkely : ServerBehavior
{
	public override async void OnReady()
	{
		print(isConnected());
		print(isServer());

		warn("AAAAAAAAAAAAAAAAAAAAAAA");

		print("You have been grunk tinkelyed!");
	}

}
```

---

<details>
<summary><b><font size="5px">Properties</font></b></summary>

<a href="https://docs.godotengine.org/en/stable/classes/class_node.html#properties">Node Properties</a>

{% include-markdown "../../types/behavior.md" start="<!--props-->" end="<!--props_end-->" %}
{% include-markdown "../../types/singleton.md" start="<!--props-->" end="<!--props_end-->" %}

</details>

---

<details>
<summary><b><font size="5px">Methods</font></b></summary>

<a href="https://docs.godotengine.org/en/stable/classes/class_node.html#methods">Node Methods</a>

{% include-markdown "../../types/behavior.md" start="<!--funcs-->" end="<!--funcs_end-->" %}
{% include-markdown "../../types/singleton.md" start="<!--funcs-->" end="<!--funcs_end-->" %}

</details>


---