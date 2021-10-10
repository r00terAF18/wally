using HtmlAgilityPack;
using Flurl;

namespace wally
{
    public class Unsplash : BaseClass
    {
        public Unsplash(string search_term)
        {
            Console.WriteLine(search_term);
        }

        public Unsplash()
        {

        }
    }
}