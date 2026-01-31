<!--props-->
<br>

<h3> Game </h3>

<table>

    {% include-markdown "./property.md" start="<!--new-->" end="<!--new_end-->" %} 

    IsLoaded : 
    
    {% include-markdown "./classdef.md" start="<!--boolean-->" end="<!--boolean_end-->" %}
    {% include-markdown "./classdef.md" start="<!--static-->" end="<!--static_end-->" %}
    {% include-markdown "./classdef.md" start="<!--desc-->" end="<!--desc_end-->" %} 

    If the game has loaded or not using <code>await Load()</code> usually done at the beginning of runtime.

    {% include-markdown "./classdef.md" start="<!--end-->" end="<!--end_end-->" %}

</table>

<br>
<!--props_end-->
<!--funcs-->
<br>

<h3> Game </h3>

<table>

    {% include-markdown "./function.md" start="<!--new-->" end="<!--new_end-->" %} 

    Load() : 
    
    {% include-markdown "./classdef.md" start="<!--void-->" end="<!--void_end-->" %}
    {% include-markdown "./classdef.md" start="<!--async-->" end="<!--async_end-->" %}
    {% include-markdown "./classdef.md" start="<!--desc-->" end="<!--desc_end-->" %} 

    Loads the game, ran automatically at runtime.

    {% include-markdown "./classdef.md" start="<!--end-->" end="<!--end_end-->" %}

</table>

<br>
<!--funcs_end-->