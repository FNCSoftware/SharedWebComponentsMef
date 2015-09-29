namespace SharedWebComponents.Contracts
{
    //todo: make a master interface with a member for each required urlprovider and use attributes (or a bool property) to decide if it should be used.  
    //todo: That way, we can easily validate if a client has conformed to the specification at compile time.
    public interface IUrlProvider {
        string GetUrl();
    }
}
