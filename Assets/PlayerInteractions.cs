using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Setări Interacțiune")]
    public float distantaInteractiune = 4.0f;
    public float timpNecesarat = 0.5f; // Cât timp ții apăsat E (secunde)
    public float timpBufferCooldown = 0.5f; // Buffer de securitate după ce ai luat bancnota

    [Header("Referințe UI & Manager")]
    public TextMeshProUGUI textPromptUI;
    public SpawnPeBanda managerJoc;

    private Bancnota bancnotaTinta = null;
    private float cronometruTimp = 0f;
    private float cronometruCooldown = 0f; // Cronometru pentru buffer
    private bool inCooldown = false;

    void Update()
    {
        // Gestionăm timpul de buffer/cooldown
        if (inCooldown)
        {
            cronometruCooldown -= Time.deltaTime;
            if (cronometruCooldown <= 0f)
            {
                inCooldown = false;
            }
        }

        DetecteazaBancnota();
        HandleHoldE();
    }

    void DetecteazaBancnota()
    {
        Vector3 punctPornire = transform.position + transform.forward * 0.5f;
        RaycastHit[] hits = Physics.RaycastAll(punctPornire, transform.forward, distantaInteractiune);

        Bancnota gasita = null;

        foreach (RaycastHit h in hits)
        {
            Bancnota b = h.collider.GetComponentInParent<Bancnota>();
            if (b != null)
            {
                gasita = b;
                break;
            }
        }

        if (gasita != null)
        {
            bancnotaTinta = gasita;

            if (textPromptUI != null)
            {
                textPromptUI.gameObject.SetActive(true);

                // Dacă suntem în perioada de buffer (după ce am luat deja o bancnotă)
                if (inCooldown)
                {
                    textPromptUI.text = $"<color=green>✓ {gasita.numeBancnota} Adaugat!</color>";
                }
                else if (cronometruTimp > 0)
                {
                    int procent = Mathf.RoundToInt((cronometruTimp / timpNecesarat) * 100f);
                    textPromptUI.text = $"[Tine E] Preia {gasita.numeBancnota} ({procent}%)";
                }
                else
                {
                    textPromptUI.text = $"[Tine E] Preia {gasita.numeBancnota}";
                }
            }
        }
        else
        {
            bancnotaTinta = null;

            // Daca nu ne uitam la nimic, resetam timpul (dar pastram cooldown-ul activ daca e cazul)
            if (!inCooldown)
            {
                cronometruTimp = 0f;
            }

            if (textPromptUI != null)
            {
                textPromptUI.gameObject.SetActive(false);
            }
        }
    }

    void HandleHoldE()
    {
        if (bancnotaTinta == null) return;

        // Dacă suntem încă în timpul de buffer, nu lăsăm jucătorul să adauge din nou
        if (inCooldown) return;

        if (Keyboard.current != null && Keyboard.current.eKey.isPressed)
        {
            cronometruTimp += Time.deltaTime;

            if (cronometruTimp >= timpNecesarat)
            {
                // 1. Adăugăm valoarea bancnotei
                if (managerJoc != null)
                {
                    managerJoc.AdaugaRest(bancnotaTinta.valoare);
                }

                // 2. Activăm starea de Buffer / Cooldown
                inCooldown = true;
                cronometruCooldown = timpBufferCooldown;
                cronometruTimp = 0f; // Resetăm progresul
            }
        }
        else
        {
            // Dacă a eliberat tasta E înainte de 100%, resetăm progresul
            cronometruTimp = 0f;
        }
    }
}