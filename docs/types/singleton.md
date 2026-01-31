<!--props-->
<h3> Singleton </h3>

<table>

    {% include-markdown "./property.md" start="<!--new-->" end="<!--new_end-->" %} 
    Me : 
    {% include-markdown "./classdef.md" start="<!--generic-->" end="<!--generic_end-->" %},
    {% include-markdown "./classdef.md" start="<!--static-->" end="<!--static_end-->" %}
    {% include-markdown "./classdef.md" start="<!--desc-->" end="<!--desc_end-->" %} 
    Instance of the singleton, recommended to use <code class="language-csharp">await Instance()</code> because it's awaited
    {% include-markdown "./classdef.md" start="<!--close-->" end="<!--close_end-->" %}

</table>
<!--props_end-->
<!--funcs-->
<h3> Singleton </h3>

<table>

    {% include-markdown "./function.md" start="<!--new-->" end="<!--new_end-->" %} 
    Instance() : 
    {% include-markdown "./classdef.md" start="<!--generic-->" end="<!--generic_end-->" %},
    {% include-markdown "./classdef.md" start="<!--static-->" end="<!--static_end-->" %},
    {% include-markdown "./classdef.md" start="<!--async-->" end="<!--async_end-->" %}
    {% include-markdown "./classdef.md" start="<!--desc-->" end="<!--desc_end-->" %} 
    Waits until the singleton is ready before returning it.
    {% include-markdown "./classdef.md" start="<!--close-->" end="<!--close_end-->" %}

</table>
<!--funcs_end-->