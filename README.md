# Resolve Third-Party Reference Links Sandcastle Component

Sandcastle (SHFB) component to resolve third-party reference links.


## How To Use

### Deploy

Copy the [`ResolveThirdPartyReferenceLinks.dll`](https://github.com/ritchiecarroll/ResolveThirdPartyReferenceLinks/releases) assembly to the `%SHFBROOT%\Components` folder.

The `%SHFBROOT%` environment variable is set by the Sandcastle Help File Builder (SHFB) installer and points to the root folder where SHFB is installed. The default location is `C:\Program Files (x86)\EWSoftware\Sandcastle Help File Builder\`.

### Configure

Provide the `<configuration>` element in the component config when manually editing the `.shfbproj` file. Defines  all the URL providers that your project needs.

The following example provides two URL providers for two different external API docs using `formattedProvider` that generate URLs based on a format:
```xml
<ComponentConfig id="Resolve ThirdParty Reference Links" enabled="True">
    <component id="Resolve ThirdParty Reference Links">

        <configuration>
            <urlProviders>
                <!-- URL provider for Autodesk Revit API Documentation -->
                <formattedProvider title="Revit URL Provider">
                    <targetMatcher pattern="T:Autodesk\.Revit\..+" />
                    <urlFormatter format="https://api.apidocs.co/resolve/revit/{revitVersion}/?asset_id={target}" />
                    <parameters>
                        <parameter name="revitVersion" default="" />
                    </parameters>
                </formattedProvider>

                <!-- URL provider for RhinoCommon Documentation -->
                <formattedProvider title="RhinoCommon URL Provider">
                    <targetMatcher pattern="T:Rhino\.Geometry\..+" />
                    <targetFormatter>
                        <steps>
                            <replace pattern="T:" with="T_" />
                            <replace pattern="\." with="_" />
                        </steps>
                    </targetFormatter>
                    <urlFormatter format="https://developer.rhino3d.com/api/RhinoCommon/html/{target}.htm" />
                </formattedProvider>
            </urlProviders>
        </configuration>
        
        <revitVersion value="$(RevitVersion)" />

    </component>
</ComponentConfig>
```

The next example cross-links GitHub pages using Sandcastle generated content in separate repos within the same organizational site:
```xml
<ComponentConfig id="Resolve ThirdParty Reference Links" enabled="True">
  <component id="Resolve ThirdParty Reference Links">
    <configuration>
      <urlProviders>
        <formattedProvider title="Gemstone.Communication URL Provider">
          <targetMatcher pattern=".:Gemstone\.Communication(\.|$).*" fullyQualifiedMemberName="false"/>
          <targetFormatter>
            <steps>
              <!-- Replace member separator tokens with underscores to match SHFB URL targets -->
              <replace pattern=":|\.|`" with="_"/>
              <!-- Remove any method parameter definitions to match SHFB URL targets -->
              <replace pattern="\(([^\)]*)\)" with=""/>
              <!-- When sandcastleTarget is set to "true", the above settings are the default -->
            </steps>
          </targetFormatter>
          <urlFormatter format="https://gemstone.github.io/communication/help/html/{target}.htm" target="_self"/>
        </formattedProvider>
        <formattedProvider title="Gemstone.Threading URL Provider">
          <targetMatcher pattern=".:Gemstone\.Threading(\.|$).*" fullyQualifiedMemberName="false" sandcastleTarget="true"/>
          <urlFormatter format="https://gemstone.github.io/threading/help/html/{target}.htm" target="_self"/>
        </formattedProvider>
        <formattedProvider title="Gemstone.Timeseries URL Provider">
          <targetMatcher pattern=".:Gemstone\.Timeseries(\.|$).*" fullyQualifiedMemberName="false" sandcastleTarget="true"/>
          <urlFormatter format="https://gemstone.github.io/timeseries/help/html/{target}.htm" target="_self"/>
        </formattedProvider>
        <formattedProvider title="Gemstone.Diagnostics URL Provider">
          <targetMatcher pattern=".:Gemstone\.Diagnostics(\.|$).*" fullyQualifiedMemberName="false" sandcastleTarget="true"/>
          <urlFormatter format="https://gemstone.github.io/diagnostics/help/html/{target}.htm" target="_self"/>
        </formattedProvider>
        <!-- Match Gemstone IO namespace for classes not in common -->
        <formattedProvider title="Gemstone.IO URL Provider">
          <targetMatcher pattern=".:Gemstone\.IO(\.|$)(?!(?:Block|FilePath|Parsing\.I?Binary|Parsing\.Boolean|Parsing\.ISupportBinary|Parsing\.StringParser)).*" fullyQualifiedMemberName="false" sandcastleTarget="true"/>
          <urlFormatter format="https://gemstone.github.io/io/help/html/{target}.htm" target="_self"/>
        </formattedProvider>
        <formattedProvider title="Gemstone.Numeric URL Provider">
          <targetMatcher pattern=".:Gemstone\.Numeric(\.|$).*" fullyQualifiedMemberName="false"/>
          <targetFormatter>
            <steps>
              <replace pattern=":|\.|`" with="_"/>
              <replace pattern="\(([^\)]*)\)" with=""/>
            </steps>
          </targetFormatter>
          <urlFormatter format="https://gemstone.github.io/numeric/help/html/{target}.htm" target="_self"/>
        </formattedProvider>
        <!-- Add Gemstone root namespace last because it has the widest match criteria -->
        <!-- Pattern excludes target namespace so local memmbers are not redirected to common -->
        <formattedProvider title="Gemstone Common URL Provider">
          <targetMatcher pattern=".:Gemstone\.(?!PhasorProtocols(\.|$)).*" fullyQualifiedMemberName="false"/>
          <targetFormatter>
            <steps>
              <replace pattern=":|\.|`" with="_"/>
              <replace pattern="\(([^\)]*)\)" with=""/>
            </steps>
          </targetFormatter>
          <urlFormatter format="https://gemstone.github.io/common/help/html/{target}.htm" target="_self"/>
        </formattedProvider>
      </urlProviders>
    </configuration>
  </component>
</ComponentConfig>
```

### URL Formatters

#### All URL formatters have:

* `title` attribute.
* `targetMatcher.pattern` attribute: a regular expression that matches with the given key provided by Sandcastle e.g `T:Autodesk.Revit.DB.Color`. If this matches the given key, then the provider will be used.
* `targetMatcher.fullyQualifiedMemberName` attribute: a boolean that indicates if the given type name is fully qualified when used as anchor text. For example, `Autodesk.Revit.DB.Color` is fully qualified with a namespace, but `Color` is not -- _defaults to `true`_.
* `targetMatcher.sandcastleTarget` attribute: a boolean that indicates if the given target is a Sandcastle documentation target. When `true`, then the provider will be configured for Sandcastle rendered documentation sites which automatically detects method overloads and properly renders URL targets for the specific overloads, e.g., `_1` or `_2` URL suffixes for second and third method overload pages, respectively. Additionally, if no target formatter steps are defined, the default steps will be set to replace `` :|\.|` `` with `_` and remove `\(([^\)]*)\)` when `sandcastleTarget` is `true` -- _defaults to `false`_.
* `parameters`: a collection of `parameter` elements that are inputs provided to the formatter, from the component configurations in Sandcastle e.g. `revitVersion` in the example above. This allows URL provides to use custom parameters set during build.


#### URL Format Provider (`formattedProvider`):

The URL format provider generates URLs based on a given format:

* `targetFormatter`: set of steps to format the given type name e.g. `T:Rhino.Geometry.Curve` to `T_Rhino_Geometry_Curve` in example above.
* `urlFormatter.format`: url format that includes `{}` tags for `target` and other parameters e.g. See `{revitVersion}` in the example above.
* `urlFormatter.target`: target window for the URL e.g. `_self` in the example above -- _defaults to `_blank`_.
* `urlFormatter.rel`: relative path to use for the anchor -- _defaults to `noreferrer`_.

 #### Limitations

* When targeting links to Sandcastle generated docs, it is easiest to create regular expressions for remote sites that use "member names" for topic file naming. Otherwise, explicit `targetFormatter` configuration sections for each target may be required.

* For classes exposing generic methods from inherited types, resolving the correct type from the provided type info is not directly possible. In these cases the provided generic method type is labeled with a tick and an index, e.g., `` `0`` or `` `1``, so the URL method title for type is simply rendered as `T1` or `T2`, respectively. This is usually not a huge issue since the generic type is described in the method summary and the method parameters are still rendered correctly.

## Thanks

- [Eric Woodruff](https://github.com/EWSoftware/SHFB) for Sandcastle (SHFB)
- [Sand Castle](https://icons8.com/icon/Y8hpNo5KuUdv/sand-castle) icon by [Icons8](https://icons8.com)
