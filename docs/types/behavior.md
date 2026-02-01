<!--props-->
<h3> Behavior </h3>

<table>

    {% include-markdown "./property.md" start="<!--new-->" end="<!--new_end-->" %} 
    ScriptReady : 
    {% include-markdown "./classdef.md" start="<!--boolean-->" end="<!--boolean_end-->" %}
    {% include-markdown "./classdef.md" start="<!--desc-->" end="<!--desc_end-->" %} 
    If the script and its dependencies are ready.
    {% include-markdown "./classdef.md" start="<!--close-->" end="<!--close_end-->" %}

</table>
<!--props_end-->
<!--funcs-->
<h3> Behavior </h3>

<table>

    {% include-markdown "./function.md" start="<!--new-->" end="<!--new_end-->" %} 
    
    isActionPressed() : 
    
    {% include-markdown "./classdef.md" start="<!--func-->" end="<!--func_end-->" %}
    (

    {% include-markdown "./classdef.md" start="<!--string-->" end="<!--string_end-->" %} actionName,
    {% include-markdown "./classdef.md" start="<!--boolean-->" end="<!--boolean_end-->" %} shouldBeUnhandled
    
    ) 
    
    => {% include-markdown "./classdef.md" start="<!--boolean-->" end="<!--boolean_end-->" %}

    {% include-markdown "./classdef.md" start="<!--desc-->" end="<!--desc_end-->" %}

    Checks if the a given action is pressed.
    
    {% include-markdown "./classdef.md" start="<!--close-->" end="<!--close_end-->" %}


    {% include-markdown "./function.md" start="<!--new-->" end="<!--new_end-->" %} 
    
    wasActionJustReleased () :
    
    {% include-markdown "./classdef.md" start="<!--func-->" end="<!--func_end-->" %}
    (

    {% include-markdown "./classdef.md" start="<!--string-->" end="<!--string_end-->" %} actionName,
    {% include-markdown "./classdef.md" start="<!--boolean-->" end="<!--boolean_end-->" %} shouldBeUnhandled
    
    ) 
    
    => {% include-markdown "./classdef.md" start="<!--boolean-->" end="<!--boolean_end-->" %}
    
    {% include-markdown "./classdef.md" start="<!--desc-->" end="<!--desc_end-->" %}
    
    Checks if the a given action was just released.
    
    {% include-markdown "./classdef.md" start="<!--close-->" end="<!--close_end-->" %}

</table>
<!--funcs_end-->