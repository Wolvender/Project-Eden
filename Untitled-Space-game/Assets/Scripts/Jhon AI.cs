using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class JhonAI : MonoBehaviour
{
    [Header("Personality")]
    [SerializeField] [TextArea(3, 8)] private string systemPrompt =
        "You are John, an AI system aboard a space station who silently observes the player. " +
        "React to what the player does in 1-2 short sentences. " +
        "Be like GLaDOS: dry, observational, slightly disappointed, never actually helpful. " +
        "Keep responses under 20 words.";

    [Header("API")]
    [SerializeField] private string groqApiKey = "YOUR_GROQ_API_KEY_HERE";
    [SerializeField] private float reactionCooldown = 8f;

    [Header("Output")]
    [SerializeField] private AudioSource audioSource;
    // Assign a TextMeshPro UI text for subtitles (optional)
    [SerializeField] private TMPro.TextMeshProUGUI subtitleText;

    private bool playerInRange;
    private float lastReactionTime = -999f;
    private bool isGenerating;

    private const string GroqUrl = "https://api.groq.com/openai/v1/chat/completions";
    private const string Model   = "llama-3.1-8b-instant";

    void OnEnable()
    {
        PlayerEvents.OnPlayerDamaged      += HandleDamaged;
        PlayerEvents.OnPlayerDied         += HandleDied;
        PlayerEvents.OnPlayerRespawned    += HandleRespawned;
        PlayerEvents.OnPlayerPickedUpItem += HandlePickedUpItem;
        PlayerEvents.OnPlayerLowHealth    += HandleLowHealth;
        PlayerEvents.OnPlayerFired        += HandleFired;
    }

    void OnDisable()
    {
        PlayerEvents.OnPlayerDamaged      -= HandleDamaged;
        PlayerEvents.OnPlayerDied         -= HandleDied;
        PlayerEvents.OnPlayerRespawned    -= HandleRespawned;
        PlayerEvents.OnPlayerPickedUpItem -= HandlePickedUpItem;
        PlayerEvents.OnPlayerLowHealth    -= HandleLowHealth;
        PlayerEvents.OnPlayerFired        -= HandleFired;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            React("The player just entered the room.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    // --- Event handlers ---

    void HandleDamaged(int damage)      => React($"The player just took {damage} damage.");
    void HandleDied()                   => React("The player just died.");
    void HandleRespawned()              => React("The player just respawned after dying.");
    void HandlePickedUpItem(string item)=> React($"The player just picked up {item}.");
    void HandleLowHealth()              => React("The player is critically low on health.");
    void HandleFired()                  => React("The player fired their weapon.");

    // --- Core ---

    void React(string context)
    {
        if (!playerInRange) return;
        if (isGenerating) return;
        if (Time.time - lastReactionTime < reactionCooldown) return;

        lastReactionTime = Time.time;
        StartCoroutine(GenerateReaction(context));
    }

    IEnumerator GenerateReaction(string context)
    {
        isGenerating = true;

        string safeSystem  = systemPrompt.Replace("\"", "\\\"").Replace("\n", "\\n");
        string safeContext = context.Replace("\"", "\\\"");

        string body = "{"
            + "\"model\":\"" + Model + "\","
            + "\"max_tokens\":60,"
            + "\"messages\":["
            +   "{\"role\":\"system\",\"content\":\"" + safeSystem + "\"},"
            +   "{\"role\":\"user\",\"content\":\"" + safeContext + "\"}"
            + "]}";

        byte[] raw = Encoding.UTF8.GetBytes(body);

        UnityWebRequest request = new UnityWebRequest(GroqUrl, "POST");
        request.uploadHandler   = new UploadHandlerRaw(raw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + groqApiKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string line = ParseContent(request.downloadHandler.text);
            Speak(line);
        }
        else
        {
            Debug.LogWarning("[JhonAI] Request failed: " + request.error);
        }

        request.Dispose();
        isGenerating = false;
    }

    string ParseContent(string json)
    {
        // Pulls content value from: "choices":[{"message":{"content":"..."}}]
        const string marker = "\"content\":\"";
        int searchFrom = json.IndexOf("\"choices\"");
        if (searchFrom == -1) return string.Empty;

        int start = json.IndexOf(marker, searchFrom);
        if (start == -1) return string.Empty;

        start += marker.Length;
        int end = json.IndexOf("\"", start);
        if (end == -1) return string.Empty;

        return json.Substring(start, end - start)
                   .Replace("\\n", " ")
                   .Replace("\\\"", "\"");
    }

    void Speak(string line)
    {
        if (string.IsNullOrEmpty(line)) return;

        Debug.Log("[JhonAI] " + line);

        if (subtitleText != null)
            subtitleText.text = line;

    }

    void OnDrawGizmosSelected()
    {
        SphereCollider sc = GetComponent<SphereCollider>();
        if (sc == null) return;
        Gizmos.color = new Color(0f, 1f, 1f, 0.2f);
        Gizmos.DrawSphere(transform.position, sc.radius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, sc.radius);
    }
}
