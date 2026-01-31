<!--props-->
<h3> Game </h3>

<table>

    {% include-markdown "./property.md" start="<!--new-->" end="<!--new_end-->" %} 
    IsLoaded : 
    {% include-markdown "./classdef.md" start="<!--boolean-->" end="<!--boolean_end-->" %},
    {% include-markdown "./classdef.md" start="<!--static-->" end="<!--static_end-->" %}
    {% include-markdown "./classdef.md" start="<!--desc-->" end="<!--desc_end-->" %} 
    If the game has loaded or not using <code>await Load()</code> usually done at the beginning of runtime.
    {% include-markdown "./classdef.md" start="<!--close-->" end="<!--close_end-->" %}

</table>
<!--props_end-->
<!--funcs-->
<h3> Game </h3>

<table>

    {% include-markdown "./function.md" start="<!--new-->" end="<!--new_end-->" %} 
    Load() : 
    {% include-markdown "./classdef.md" start="<!--void-->" end="<!--void_end-->" %},
    {% include-markdown "./classdef.md" start="<!--async-->" end="<!--async_end-->" %}
    {% include-markdown "./classdef.md" start="<!--desc-->" end="<!--desc_end-->" %}
    Loads the game, ran automatically at runtime.
    {% include-markdown "./classdef.md" start="<!--close-->" end="<!--close_end-->" %}


    {% include-markdown "./function.md" start="<!--new-->" end="<!--new_end-->" %} 
    GetSystem<<code><a href="../../classes/singletons/IBaseSingleton">IBaseSingleton</a></code>>() : 
    {% include-markdown "./classdef.md" start="<!--generic-->" end="<!--generic_end-->" %},
    {% include-markdown "./classdef.md" start="<!--async-->" end="<!--async_end-->" %}
    {% include-markdown "./classdef.md" start="<!--desc-->" end="<!--desc_end-->" %} 
    Gets a singleton system by its generic type

```cs
var guis = Game.GetSystem<GuiSystem>();
```

    {% include-markdown "./classdef.md" start="<!--close-->" end="<!--close_end-->" %}


    {% include-markdown "./function.md" start="<!--new-->" end="<!--new_end-->" %} 
    GetSystems<<code><a href="../../classes/singletons/IBaseSingleton">IBaseSingleton</a></code>...>() : 
    {% include-markdown "./classdef.md" start="<!--tuple-->" end="<!--tuple_end-->" %}<{% include-markdown "./classdef.md" start="<!--generic-->" end="<!--generic_end-->" %}>,
    {% include-markdown "./classdef.md" start="<!--async-->" end="<!--async_end-->" %}
    {% include-markdown "./classdef.md" start="<!--desc-->" end="<!--desc_end-->" %} 
    Gets up to 10 singleton systems by their generic types

```cs
var (guis, workspace, server) = Game.GetSystems<GuiSystem, Workspace, Server>();
```

    {% include-markdown "./classdef.md" start="<!--close-->" end="<!--close_end-->" %}

</table>
<!--funcs_end-->
<!--events-->
<h3> Game </h3>

<table>

    {% include-markdown "./event.md" start="<!--new-->" end="<!--new_end-->" %} 
    StartedHosting : {% include-markdown "./classdef.md" start="<!--signal-->" end="<!--signal_end-->" %}( {% include-markdown "./classdef.md" start="<!--string-->" end="<!--string_end-->" %} id )
    {% include-markdown "./classdef.md" start="<!--desc-->" end="<!--desc_end-->" %} 
    Fired when the local player starts hosting
    {% include-markdown "./classdef.md" start="<!--close-->" end="<!--close_end-->" %}

</table>
<!--events_end-->