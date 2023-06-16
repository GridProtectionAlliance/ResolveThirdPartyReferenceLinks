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

The next example cross-links GitHub pages using Sandcastle generated conntent in separate repos within the same organizational site:
```xml
  <ComponentConfig id="Resolve ThirdParty Reference Links" enabled="True">
    <component id="Resolve ThirdParty Reference Links">
      <configuration>
        <urlProviders>
          <formattedProvider title="Gemstone.Communication URL Provider">
            <targetMatcher pattern=".:Gemstone\.Communication\..+" fullyQualifiedMemberName="false" />
            <targetFormatter>
              <steps>
                <replace pattern="T:" with="T_" />
                <replace pattern="E:" with="E_" />
                <replace pattern="M:" with="M_" />
                <replace pattern="P:" with="P_" />
                <replace pattern="\." with="_" />
              </steps>
            </targetFormatter>
            <urlFormatter format="https://gemstone.github.io/communication/help/html/{target}.htm" target="_self" />
          </formattedProvider>
          <formattedProvider title="Gemstone.Threading URL Provider">
            <targetMatcher pattern=".:Gemstone\.Threading\..+" fullyQualifiedMemberName="false" />
            <targetFormatter>
              <steps>
                <replace pattern="T:" with="T_" />
                <replace pattern="E:" with="E_" />
                <replace pattern="M:" with="M_" />
                <replace pattern="P:" with="P_" />
                <replace pattern="\." with="_" />
              </steps>
            </targetFormatter>
            <urlFormatter format="https://gemstone.github.io/threading/help/html/{target}.htm" target="_self" />
          </formattedProvider>
          <formattedProvider title="Gemstone.Timeseries URL Provider">
            <targetMatcher pattern=".:Gemstone\.Timeseries\..+" fullyQualifiedMemberName="false" />
            <targetFormatter>
              <steps>
                <replace pattern="T:" with="T_" />
                <replace pattern="E:" with="E_" />
                <replace pattern="M:" with="M_" />
                <replace pattern="P:" with="P_" />
                <replace pattern="\." with="_" />
              </steps>
            </targetFormatter>
            <urlFormatter format="https://gemstone.github.io/timeseries/help/html/{target}.htm" target="_self" />
          </formattedProvider>
          <formattedProvider title="Gemstone.Diagnostics URL Provider">
            <targetMatcher pattern=".:Gemstone\.Diagnostics\..+" fullyQualifiedMemberName="false" />
            <targetFormatter>
              <steps>
                <replace pattern="T:" with="T_" />
                <replace pattern="E:" with="E_" />
                <replace pattern="M:" with="M_" />
                <replace pattern="P:" with="P_" />
                <replace pattern="\." with="_" />
              </steps>
            </targetFormatter>
            <urlFormatter format="https://gemstone.github.io/diagnostics/help/html/{target}.htm" target="_self" />
          </formattedProvider>
          <formattedProvider title="Gemstone.IO URL Provider">
            <targetMatcher pattern=".:Gemstone\.IO\.(?!Parsing\.).+" fullyQualifiedMemberName="false" />
            <targetFormatter>
              <steps>
                <replace pattern="T:" with="T_" />
                <replace pattern="E:" with="E_" />
                <replace pattern="M:" with="M_" />
                <replace pattern="P:" with="P_" />
                <replace pattern="\." with="_" />
              </steps>
            </targetFormatter>
            <urlFormatter format="https://gemstone.github.io/io/help/html/{target}.htm" target="_self" />
          </formattedProvider>
          <formattedProvider title="Gemstone.Numeric URL Provider">
            <targetMatcher pattern=".:Gemstone\.Numeric\..+" fullyQualifiedMemberName="false" />
            <targetFormatter>
              <steps>
                <replace pattern="T:" with="T_" />
                <replace pattern="E:" with="E_" />
                <replace pattern="M:" with="M_" />
                <replace pattern="P:" with="P_" />
                <replace pattern="\." with="_" />
              </steps>
            </targetFormatter>
            <urlFormatter format="https://gemstone.github.io/numeric/help/html/{target}.htm" target="_self" />
          </formattedProvider>
          <!-- Add Gemstone root namespace last because it has the widest match criteria -->
          <!-- Pattern excludes target namespace so local memmbers are not redirected to common -->
          <formattedProvider title="Gemstone Common URL Provider">
            <targetMatcher pattern=".:Gemstone\.(?!PhasorProtocols\.)..+" fullyQualifiedMemberName="false" />
            <targetFormatter>
              <steps>
                <replace pattern="T:" with="T_" />
                <replace pattern="E:" with="E_" />
                <replace pattern="M:" with="M_" />
                <replace pattern="P:" with="P_" />
                <replace pattern="\." with="_" />
              </steps>
            </targetFormatter>
            <urlFormatter format="https://gemstone.github.io/common/help/html/{target}.htm" target="_self" />
          </formattedProvider>
        </urlProviders>
      </configuration>
    </component>
  </ComponentConfig>
```

### URL Formatters

#### All URL formatters have:

- `title` attribute.
- `targetMatcher.pattern` attribute: is a regular expression that matches with the given key provided by Sandcastle e.g `T:Autodesk.Revit.DB.Color`. If this matches the given key, then the provider will be used.
 - 'targetMatcher.fullyQualifiedMemberName' attribute: is a boolean that indicates if the given type name is fully qualified when used as anchor text. For example, `Autodesk.Revit.DB.Color` is fully qualified with a namespace, but `Color` is not -- _defaults to `true`_.
- `parameters`: is a collection of `parameter` elements that are inputs provided to the formatter, from the component configurations in Sandcastle e.g. `revitVersion` in the example above. This allows URL provides to use custom parameters set during build.

#### FormatterUrlProvider (`formattedProvider`):

FormatterUrlProvider generates URLs based on a given format:

- `targetFormatter`: is a set of steps to format the given type name e.g. `T:Rhino.Geometry.Curve` to `T_Rhino_Geometry_Curve` in example above.
- `urlFormatter.format`: is the url format that includes `{}` tags for `target` and other parameters e.g. See `{revitVersion}` in the example above.
 - `urlFormatter.target`: is the target window for the URL e.g. `_self` in the example above -- _defaults to `_blank`_.
 - `urlFormatter.rel`: is the relative path to use for the anchor -- _defaults to `noreferrer`_.


## Thanks

- [Eric Woodruff](https://github.com/EWSoftware/SHFB) for Sandcastle (SHFB)
- [Sand Castle](https://icons8.com/icon/Y8hpNo5KuUdv/sand-castle) icon by [Icons8](https://icons8.com)