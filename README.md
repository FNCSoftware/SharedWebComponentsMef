# SharedWebComponentsMef
A prototype that demonstrates what a plugin-based architecture using MEF looks like.

# Current features: #

- Accepts client dll’s by a naming convention (currently using clientName.anything) where clientName will be used for routing (ie clientName/controller/action/id)
- Uses MEF conventions to extract implementations of IController (built-in MVC controller contract) and IUrlResolver (custom implementation that will be used as bridge between main app and client implementations)
- Has a controller factory that prefers controllers from main project to client controllers
- Can extract views (haven’t done other resources yet) correctly from client dll’s
- Can extract resources from the correct dll given that multiple dll’s are present and they contain resources with the same name

# To set up a new client: #

1. Create solution that includes a client MVC project and SharedWebComponents project
2. Add sections to base web.config of client MVC project to automatically mark resources as embedded (as seen in Client1.Page and Client2.Page)
3. Add call to MefConfig.Register() in Application_Start()
