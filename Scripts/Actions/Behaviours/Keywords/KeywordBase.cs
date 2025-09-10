using System.Threading.Tasks;

/// <summary>
/// A keyword adds additional effects or modifications to actions like spells or attacks.
/// These are modular and can be reused across different actions.
/// </summary>
public class KeywordBase
{
    /*
    *   Add methods here as needed for keyword effects
    *   Keywords should be simple and understandable on a glance.
    *   They should be reusable and not tied to specific spells or actions.
    */
    
    public async virtual Task OnCast()
    {
        // Default implementation does nothing
    }

    public async virtual Task OnHit()
    {
        // Default implementation does nothing
    }
}
