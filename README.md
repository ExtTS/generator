![TypeScript](https://img.shields.io/badge/TypeScript->=3.7-brightgreen.svg?style=plastic)
[![License](https://img.shields.io/badge/Licence-BSD3-brightgreen.svg?style=plastic)](https://raw.githubusercontent.com/ExtTS/generator/master/LICENSE)

# Ext.JS Framework TypeScript Type Definitions Generator

<div align="center">
  
![Ext.JS TS Types Generator](https://raw.githubusercontent.com/ExtTS/generator/master/ExtTsTypesGenerator/App/gfx/printscreen.png)

</div>

## Examples How To Use Ext.JS TypeScript Definitions
Best way to understand how to use the definitions are examples here:  
- [Example in Ext.JS 5.0.1](https://github.com/ExtTS/example-project-501)  
- [Example in Ext.JS 6.0.1 classic](https://github.com/ExtTS/example-project-601-classic)  
- [Example in Ext.JS 6.0.1 modern](https://github.com/ExtTS/example-project-601-modern)  
- [**Example in Ext.JS 6.2.0 classic**](https://github.com/ExtTS/example-project-620-classic)  
- [Example in Ext.JS 6.2.0 modern](https://github.com/ExtTS/example-project-620-modern)  
- [Example in Ext.JS 7.0.0 classic](https://github.com/ExtTS/example-project-700-classic)  
- [Example in Ext.JS 7.0.0 modern](https://github.com/ExtTS/example-project-700-modern)  

## Features
- Unzips necessary source files, parse (by JS Duck) and generates TypeScript definition files.
- Generates definitions as single file or in multiple files (not to work with one very large file).
- Generate type definitions separately only for classic toolkit or only for modern toolkit, not together, it's not possible yet, not implemented.
- Generates all classes, interfaces and members with optional JS Docs.
- Generates definitions with optional custom documentation base URL 
  to generate JS Docs links into your custom offline documentation.
- Generates definitions with no access modifiers like `private`, `protected` or `public`,
  because then there is not possible to extend classes like that into interfaces to declare
  objects to extend class. Access modifiers are declared in JS Docs as `@private`, 
  `@protected` or `@public`.
- Generates definitions always for Ext.JS core, optionally for chosen packages.
- Generates definitions for:
  - All Ext.JS framework classes as it is (including singletons as it is) like:
    - `Ext`, `Ext.Array`, `Ext.panel.Panel` etc...
  - Interfaces for configuration objects (to create classes by `Ext.create()`) like this:
    - ```ts
      Ext.create('Ext.panel.Panel', <Ext.panel.Panel.Cfg>{
         title: `My Panel Title`,
         closable: true
      });
      ```
  - Interfaces to define extended classes like this:
    - ```ts
      Ext.define('App.controller.Main', <Ext.app.Controller.Def>{
         extend: 'Ext.app.Controller',
         config: <Ext.app.Controller.Cfg>{
            refs[{
               ref: 'mainTabs'
               selector: 'tabpanel[cls=main-tabs]'
            }]
         }
      });
      ```
  - Interfaces for events objects (to define `listeners` property)
    - ```ts
      Ext.define('App.view.MainTab', <Ext.panel.Panel.Def & Ext.panel.Panel.Cfg>{
         extend: 'Ext.panel.Panel',
         opened: true,
         listeners: <Ext.panel.Panel.Events>{
            close: function (panel: Ext.panel.Panel, eOpts: object) {
               this.opened = false;
            }
         }
      });
      ```
  - Interfaces for all structured method params
    - ```ts
      Ext.Ajax.request(<Ext.data.Connection.methodParams.request.Options>{
         url: "?submit",
         success: function (response?: object, options?: object) {
             console.log(response);
         }
      });
      ```
  - Callbacks, mixed things etc...
  - Generates a few unknown empty classes definitions in separate file named `ext-X.X.X-classic-unknown.d.ts`.
  - Generated definitions could run automcompleting anywhere in your application, more info in this blog post:  
    https://blorkfish.wordpress.com/2013/01/28/using-extjs-with-typescript/
  - Generated definitions supports refactoring and code navigation in Visual Studio:-)

## Tetsted Ext.JS Versions And Toolkits
- v4.x.x and lower - NOT TESTED, there might be some problems, let me know or make a pull request
- **v5.0.1 classic - TESTED, working OK**
- **v6.0.1 classic - TESTED, working OK**
- **v6.0.1 modern - TESTED, working OK**
- **v6.2.0 classic - TESTED, working OK**
- **v6.2.0 modern - TESTED, working OK** (1 error fixed manually)
- **v7.0.0 classic - TESTED, working OK**
- **v7.0.0 modern - TESTED, working OK** (5 errors fixed manually)
