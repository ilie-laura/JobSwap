using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;

public class SpawnPeBanda : MonoBehaviour
{
    [Header("Referințe Bandă & Oprire")]
    public Transform banda;
    public Transform stopbanda;

    [Header("Clienți (Coadă)")]
    public Transform punctPornireClient;
    public Transform punctOprireClient;
    public GameObject[] prefabsClienti; // 0 = Normal, 1 = Karen
    public int marimeCoada = 3;         
    public float distantaIntreClienti = 1.5f; 

    [Header("Audio (Muzică & SFX)")]
    public AudioSource audioSourceMuzica;
    public AudioSource audioSourceSFX;
    public AudioClip muzicaNormal;
    public AudioClip muzicaKaren;
    public AudioClip sunetBeepScanare;
    public AudioClip sunetGlitchCaptcha; 

    [Header("Produse (Assets)")]
    public GameObject[] prefabsProduse;
    public int minProduse = 2;
    public int maxProduse = 4;
    public float spatiereProduse = 0.4f;

    [Header("Ajustare Produse")]
    public Vector3 scaraProdus = new Vector3(1f, 1f, 1f);
    public float inaltimeDeasupraBanda = 0.35f;

    [Header("Setări Mișcare")]
    public float vitezaBanda = 0.8f;
    public float vitezaClient = 2.0f;
    public float distantaOprireProduse = 0.3f;

    [Header("Ecrane Text")]
    public TextMeshPro ecranText;               // Ecranul 3D de pe casa de marcat
    public TextMeshProUGUI textReplicaKarenUI;  // HUD UI pe ecranul jucătorului

    [Header("Setări Karen / Limită Timp")]
    public float timpMaximKaren = 25.0f;
    private float timpRamasKaren;

    // Stări Joc & Karen Timer
    private float totalClientCurent = 0f;
    private float baniPrimitiDeLaClient = 0f;
    private float restNecesar = 0f;
    private float restOferitDeJucator = 0f;

    private float scorTotalJoc = 0f;
    private bool fazaScanare = true;
    private bool fazaRest = false;
    private bool gameOver = false;
    private bool esteKaren = false;

    // Stări CAPTCHA (Non-Uman)
    private bool captchaActiv = false;
    private int tastaTintaCaptcha = 1;

    // Sistem Coadă Clienți
    private List<GameObject> coadaClienti = new List<GameObject>();
    private List<int> tipuriClientiCoada = new List<int>(); // 0 = Normal, 1 = Karen

    // Stări interne
    private List<GameObject> produsePeBanda = new List<GameObject>();
    private bool clientLaCasa = false;
    private bool produseOprite = false;

    private string[] repliciKaren = new string[]
    {
        "\"Mi se plictiseste copilul!\"",
        "\"Vreau să vorbesc cu managerul!\"",
        "\"De ce durează atât?!\"",
        "\"Sunt client fidel, grăbește-te!\""
    };
    private string replicaCurentaKaren = "";

    void Start()
    {
        
        StartCoroutine(PopuleazaCoadaCuDelay());
    }
    
    System.Collections.IEnumerator PopuleazaCoadaCuDelay()
    {
        for (int i = 0; i < marimeCoada; i++)
        {
            AdaugaClientInCoada();

            // Dacă e primul client, începem jocul cu el imediat
            if (i == 0)
            {
                GenereazaClientNou();
            }

            
            yield return new WaitForSeconds(2.0f);
        }
    }

    void Update()
    {
        if (gameOver) return;

        ActualizeazaPozitiiCoada();

        if (captchaActiv)
        {
            HandleCaptchaInput();
            return;
        }

        HandleMiscaProduse();

        if (clientLaCasa)
        {
            timpRamasKaren -= Time.deltaTime;

            if (timpRamasKaren <= 0f)
            {
                timpRamasKaren = 0f;
                KarenTimeOut();
                return;
            }

            if (esteKaren)
            {
                ActualizeazaUIKarenScreen();
            }
        }

        if (fazaScanare)
        {
            HandleScanareProduse();
        }
        else if (fazaRest)
        {
            HandlePredareRest();
        }
    }

    void PopuleazaCoadaInitiala()
    {
        for (int i = 0; i < marimeCoada; i++)
        {
            AdaugaClientInCoada();
        }
    }

    void AdaugaClientInCoada()
    {
        int indexClient = Random.Range(0, prefabsClienti.Length);

       
        GameObject nouClient = Instantiate(prefabsClienti[indexClient], punctPornireClient.position, punctPornireClient.rotation);

        coadaClienti.Add(nouClient);
        tipuriClientiCoada.Add(indexClient);
    }
    void ActualizeazaPozitiiCoada()
    {
        
        Vector3 directieMers = (punctOprireClient.position - punctPornireClient.position).normalized;

        for (int i = 0; i < coadaClienti.Count; i++)
        {
            if (coadaClienti[i] == null) continue;

           
            Vector3 pozitieTinta = punctOprireClient.position - (directieMers * i * distantaIntreClienti);

            coadaClienti[i].transform.position = Vector3.MoveTowards(
                coadaClienti[i].transform.position,
                pozitieTinta,
                vitezaClient * Time.deltaTime
            );

           
            if (directieMers != Vector3.zero)
            {
                Quaternion rotatieTinta = Quaternion.LookRotation(directieMers);
                coadaClienti[i].transform.rotation = Quaternion.Slerp(coadaClienti[i].transform.rotation, rotatieTinta, Time.deltaTime * 5f);
            }

           
            if (i == 0 && Vector3.Distance(coadaClienti[0].transform.position, punctOprireClient.position) < 0.1f)
            {
                clientLaCasa = true;
            }
        }
    }

    public void GenereazaClientNou()
    {
        CurataBanda();

        totalClientCurent = 0f;
        baniPrimitiDeLaClient = 0f;
        restNecesar = 0f;
        restOferitDeJucator = 0f;
        fazaScanare = true;
        fazaRest = false;
        captchaActiv = false;
        produseOprite = false;
        clientLaCasa = false;

        timpRamasKaren = timpMaximKaren;

        int indexClient = tipuriClientiCoada[0];
        esteKaren = (indexClient == 1);

        if (esteKaren)
        {
            replicaCurentaKaren = repliciKaren[Random.Range(0, repliciKaren.Length)];
        }
        else
        {
            if (textReplicaKarenUI != null)
                textReplicaKarenUI.gameObject.SetActive(false);
        }

        SchimbaMuzicaClient(indexClient);

        int numarProduse = Random.Range(minProduse, maxProduse + 1);
        float startOffset = -((numarProduse - 1) * spatiereProduse) / 2f;

        for (int i = 0; i < numarProduse; i++)
        {
            int indexProdus = Random.Range(0, prefabsProduse.Length);
            GameObject prefabAles = prefabsProduse[indexProdus];

            Vector3 pozitieGlobala = banda.position
                                   + (banda.up * inaltimeDeasupraBanda)
                                   + (banda.right * (startOffset + (i * spatiereProduse)));

            GameObject produsCreat = Instantiate(prefabAles, pozitieGlobala, Quaternion.identity);

            produsCreat.transform.localScale = scaraProdus;
            produsCreat.transform.rotation = banda.rotation;
            produsCreat.transform.SetParent(banda, true);

            produsePeBanda.Add(produsCreat);
        }

        ActualizeazaEcran("Porneste scanarea\nApasa E!");
    }

    void SchimbaMuzicaClient(int indexClient)
    {
        if (audioSourceMuzica == null) return;

        // Alegem melodia în funcție de client (Index 1 = Karen)
        AudioClip muzicaDeRedat = (indexClient == 1) ? muzicaKaren : muzicaNormal;

        
        if (audioSourceMuzica.clip != muzicaDeRedat)
        {
            audioSourceMuzica.clip = muzicaDeRedat;
            audioSourceMuzica.loop = true;
            audioSourceMuzica.Play();
        }
        else
        {
            
            if (!audioSourceMuzica.isPlaying)
            {
                audioSourceMuzica.Play();
            }
        }
    }

    void HandleMiscaProduse()
    {
        if (produsePeBanda.Count == 0 || produseOprite) return;

        if (stopbanda != null && produsePeBanda[0] != null)
        {
            float distanta = Vector3.Distance(produsePeBanda[0].transform.position, stopbanda.position);
            if (distanta <= distantaOprireProduse)
            {
                produseOprite = true;
                return;
            }
        }

        foreach (GameObject prod in produsePeBanda)
        {
            if (prod != null)
            {
                prod.transform.Translate(-Vector3.right * vitezaBanda * Time.deltaTime, Space.Self);
            }
        }
    }

    void HandleScanareProduse()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (produseOprite && produsePeBanda.Count > 0)
            {
                GameObject produsScanat = produsePeBanda[0];
                Produs infoProdus = produsScanat.GetComponent<Produs>();

                string nume = "Produs Organism";
                float pret = 1.0f;

                if (infoProdus != null)
                {
                    nume = infoProdus.numeProdus;
                    pret = infoProdus.pret;
                }

                totalClientCurent += pret;

                if (!esteKaren && audioSourceSFX != null && sunetBeepScanare != null)
                {
                    CancelInvoke("OpresteBeep");
                    audioSourceSFX.clip = sunetBeepScanare;
                    audioSourceSFX.Play();
                    Invoke("OpresteBeep", 0.50f);
                }

                produsePeBanda.RemoveAt(0);
                Destroy(produsScanat);

                if (produsePeBanda.Count > 0 && Random.value < 0.4f)
                {
                    DeclanseazaCaptcha();
                    return;
                }

                if (produsePeBanda.Count == 0)
                {
                    IncepeFazaRest();
                }
                else
                {
                    ActualizeazaEcran($"{nume}: {pret} LEI\nTOTAL: {totalClientCurent} LEI");
                    produseOprite = false;
                }
            }
        }
    }

    void DeclanseazaCaptcha()
    {
        captchaActiv = true;
        tastaTintaCaptcha = Random.Range(1, 4);

        if (audioSourceSFX != null && sunetGlitchCaptcha != null)
        {
            CancelInvoke("OpresteGlitch");
            audioSourceSFX.clip = sunetGlitchCaptcha;
            audioSourceSFX.Play();
            Invoke("OpresteGlitch", 1.5f);
        }

        ActualizeazaEcran($"<color=#FF3333><b>[ VERIFICARE CAPTCHA ]</b></color>\nConfirmă că ești UMAN!\n\n<b>Apasa tasta [ {tastaTintaCaptcha} ]</b>");
    }

    void OpresteGlitch()
    {
        if (audioSourceSFX != null && audioSourceSFX.isPlaying)
        {
            audioSourceSFX.Stop();
        }
    }

    void HandleCaptchaInput()
    {
        if (Keyboard.current == null) return;

        bool tastaCorecta = false;

        if (tastaTintaCaptcha == 1 && Keyboard.current.digit1Key.wasPressedThisFrame) tastaCorecta = true;
        if (tastaTintaCaptcha == 2 && Keyboard.current.digit2Key.wasPressedThisFrame) tastaCorecta = true;
        if (tastaTintaCaptcha == 3 && Keyboard.current.digit3Key.wasPressedThisFrame) tastaCorecta = true;

        if (tastaCorecta)
        {
            captchaActiv = false;
            ActualizeazaEcran("<color=#33FF33><b>HUMAN VERIFIED!</b></color>\nContinuă scanarea...");
            produseOprite = false;
        }
    }

    void OpresteBeep()
    {
        if (audioSourceSFX != null && audioSourceSFX.isPlaying)
        {
            audioSourceSFX.Stop();
        }
    }

    void IncepeFazaRest()
    {
        fazaScanare = false;
        fazaRest = true;

        if (totalClientCurent <= 5f) baniPrimitiDeLaClient = 5f;
        else if (totalClientCurent <= 10f) baniPrimitiDeLaClient = 10f;
        else if (totalClientCurent <= 20f) baniPrimitiDeLaClient = 20f;
        else baniPrimitiDeLaClient = Mathf.Ceil(totalClientCurent / 10f) * 10f + 10f;

        restNecesar = baniPrimitiDeLaClient - totalClientCurent;

        AfiseazaStareRest();
    }

    public void AdaugaRest(float valoare)
    {
        if (!fazaRest || gameOver) return;

        restOferitDeJucator += valoare;
        AfiseazaStareRest();
    }

    void HandlePredareRest()
    {
        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            if (Mathf.Approximately(restOferitDeJucator, restNecesar) || Mathf.Abs(restOferitDeJucator - restNecesar) < 0.01f)
            {
                scorTotalJoc += totalClientCurent;
                ActualizeazaEcran("<color=#33FF33>REST CORECT!</color>\nVine alt client...");

                if (textReplicaKarenUI != null)
                {
                    textReplicaKarenUI.gameObject.SetActive(false);
                }

                // Eliminăm clientul rezolvat din coadă
                if (coadaClienti.Count > 0)
                {
                    Destroy(coadaClienti[0]);
                    coadaClienti.RemoveAt(0);
                    tipuriClientiCoada.RemoveAt(0);
                }

                // Adăugăm alt client la capătul cozii
                AdaugaClientInCoada();

                fazaRest = false;
                Invoke("GenereazaClientNou", 0.5f);
            }
            else if (restOferitDeJucator < restNecesar)
            {
                gameOver = true;
                fazaRest = false;
                if (textReplicaKarenUI != null)
                {
                    textReplicaKarenUI.gameObject.SetActive(false);
                }
                ActualizeazaEcran($"<color=#FF3333>REST GRESIT!</color>\nClientul a primit {restOferitDeJucator} LEI\nTrebuia: {restNecesar} LEI\n\n<b>GAME OVER!</b>\nSCOR FINAL: {scorTotalJoc:F2} LEI");
            }
            else
            {
                gameOver = true;
                fazaRest = false;
                if (textReplicaKarenUI != null)
                {
                    textReplicaKarenUI.gameObject.SetActive(false);
                }
                ActualizeazaEcran($"REST GRESIT!\nAi fost concediat :((");
            }
        }
    }

    void KarenTimeOut()
    {
        gameOver = true;
        fazaScanare = false;
        fazaRest = false;

        if (textReplicaKarenUI != null) textReplicaKarenUI.gameObject.SetActive(false);

        ActualizeazaEcran($"<color=#FF3333>TIMP EXPIRAT!</color>\nClientul s-a enervat și a plecat!\n\n<b>GAME OVER!</b>\nSCOR FINAL: {scorTotalJoc:F2} LEI");
    }

    void ActualizeazaUIKarenScreen()
    {
        if (textReplicaKarenUI != null)
        {
            textReplicaKarenUI.gameObject.SetActive(true);
            textReplicaKarenUI.text = $"<color=#FF3333><b>KAREN:</b></color> {replicaCurentaKaren}\n<color=#FFFF33><b>TIMP RAMAS:</b> {timpRamasKaren:F1}s</color>";
        }
    }

    void AfiseazaStareRest()
    {
        ActualizeazaEcran($"TOTAL CLIENT: {totalClientCurent} LEI\nPRIMII: {baniPrimitiDeLaClient} LEI\nREST DE DAT: {restNecesar} LEI\n----------\nREST ALES: {restOferitDeJucator} LEI\n");
    }

    void ActualizeazaEcran(string text)
    {
        if (ecranText != null)
        {
            ecranText.text = text;
        }
    }

    void CurataBanda()
    {
        foreach (GameObject prod in produsePeBanda)
        {
            if (prod != null) Destroy(prod);
        }
        produsePeBanda.Clear();
    }
}