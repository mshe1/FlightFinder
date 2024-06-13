using Microsoft.AspNetCore.Mvc;

namespace FlightFinder.Controllers {
    public class SearchController : Controller {
        public IActionResult Index(string searchTerm) {
            ViewData["SearchTerm"] = searchTerm;
            return View();
        }

        /// <summary>
        /// Adds a character to a dictionary of characters if it does not exist, increment the number of instances otherwise
        /// </summary>
        /// <param name="dict">Dictionary containing existing data</param>
        /// <param name="character">Character to add to dictionary</param>
        private void AddCharacterToDictionary(ref Dictionary<char, int> dict, char character) {
            if (dict.ContainsKey(character)) {
                dict[character]++;
            } else {
                dict.Add(character, 1);
            }
        }

        //note that searchTerm and checking the number of characters within a searched term are unnecessary for searching for the 
        //term "flight" specifically. However I've done this as though I was given the task to add this, in which case I would consider
        //the potential for re-use and the minimal additional work required to allow this.
        /// <summary>
        /// Finds the number of times that the word/phrase in searchTerm can be produced using the text in searchText. This can be in
        /// any order or configuration.
        /// </summary>
        /// <param name="searchText">The text used to produce n instances of searchTerm.</param>
        /// <param name="searchTerm">The word/phrase we are finding the number of instances of.</param>
        /// <returns>The number of instances the searchTerm can be produced using the search text.</returns>
        [HttpGet]
        public IActionResult GetPossibleInstancesOfTerm(string searchText, string searchTerm) {
            //if either parameter is empty it's not possible for there to be any matches
            if (string.IsNullOrEmpty(searchText) || string.IsNullOrEmpty(searchTerm)) {
                return Content("0");
            }
            //likewise if the search term is longer than the searched text
            if(searchTerm.Length > searchText.Length) {
                return Content("0");
            }
            //The number of matches can be in any order or combination so long as the total instances of characters in the search term
            //can be found within the searched text. Therefore the easiest way to check this is to get a count of each occurence of each
            //character from the search term within the search text and find the minimum.
            Dictionary<char, int> dictSearchTextInstances = new Dictionary<char, int>();
            //Note that this isn't necessary for the specific "flight" example. However to ensure scalability get an additional count of
            //the characters in the search term to scale with, as a word with multiple instances of the same letter will need the
            //search text to contain them multiple times. 
            Dictionary<char, int> dictSearchTermInstances = new Dictionary<char, int>();
            for(int i = 0; i < searchTerm.Length; i++) {
                AddCharacterToDictionary(ref dictSearchTermInstances, searchTerm[i]);
            }
            //Quicker to filter out uncessary characters at the end rather than checking the searchTerm for each character
            //and space usage should be minimal as the searchText is limited to 100 characters
            for(int i = 0; i < searchText.Length; i++) {
                AddCharacterToDictionary(ref dictSearchTextInstances, searchText[i]);
            }

            //must start as null not 0 as we're finding the minimum matches (which will always be greater than 0 if any)
            int? minMatches = null;
            //no need to check the same letters multiple times
            string distinctCharacters = new string(searchTerm.Distinct().ToArray());
            for(int i = 0; i < distinctCharacters.Length; i++) {
                //ensure the character occurs anywhere in the search text before attempting to get a value for it
                if (dictSearchTextInstances.ContainsKey(distinctCharacters[i])) {
                    int scaleFactor = dictSearchTermInstances[distinctCharacters[i]];
                    int characterMatches = dictSearchTextInstances[distinctCharacters[i]];
                    //need to round down to ensure partially complete words are not counted
                    int scaledMatches = (int)Math.Floor((float)characterMatches / scaleFactor);
                    if(minMatches == null || scaledMatches < minMatches) {
                        minMatches = scaledMatches;
                    }
                } else {
                    //if any character of the search term has no matches, then there are no instances of it
                    return Content("0");
                }
            }

            return Content(minMatches == null ? "0" : minMatches.Value.ToString());
        }
    }
}
