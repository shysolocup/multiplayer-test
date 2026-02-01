
---

{% include-markdown "../../types/classdef.md" start="<!--class_head-->" end="<!--class_head_end-->" %}

# Singleton3D<T\>
<sub><b> INHERITS FROM 
<code><a href="https://docs.godotengine.org/en/stable/classes/class_node.html">Node</a></code>, 
<code><a href="../IBaseSingleton">IBaseSingleton</a></code>

{% include-markdown "../../types/classdef.md" start="<!--unrep-->" end="<!--unrep_end-->" %}

Base for singleton instances extending from a <code><a href="https://docs.godotengine.org/en/stable/classes/class_node3d.html">Node3D</a></code>. Used for 3D singletons like <code><a href="../Workspace">Workspace</a></code> and <code><a href="../Characters">Characters</a></code> to contain 3D singletons.

<code>T</code> is the instance you're creating so for example [`Workspace`](../Workspace) extends from `Singleton3D<Workspace>`.

---

<details>
<summary><b><font size="5px">Properties</font></b></summary>

{% include-markdown "../../types/singleton.md" start="<!--props-->" end="<!--props_end-->" %}

</details>

---

<details>
<summary><b><font size="5px">Methods</font></b></summary>

{% include-markdown "../../types/singleton.md" start="<!--funcs-->" end="<!--funcs_end-->" %}

</details>


---