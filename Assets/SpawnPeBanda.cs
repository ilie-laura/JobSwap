using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;

public class SpawnPeBanda : MonoBehaviour
{
    [Header("Referințe Bandă & Oprire")]
    public Transform banda;
    public Transform stopbanda;

    [Header("Clienți (Customer)")]
    public Transform punctPornireClient;
    public Transform punctOprireClient;
    public GameObject[] prefabsClienti;

    [Header("Produse (Assets)")]
    public GameObject[] prefabsProduse;
    public int minProduse = 1;
    public int maxProduse = 4;
    public float spatiereProduse = 0.4f;

    [Header("Ajustare Produse")]
    public Vector3 scaraProdus = new Vector3(1f, 1f, 1f);
    public float inaltimeDeasupraBanda = 0.35f;

    [Header("Setări Mișcare")]
    public float vitezaBanda = 0.8f;
    public float vitezaClient = 1.5f;
    public float distantaOprireProduse = 0.3f;

    [Header("Ecran Casă")]
    public TextMeshPro ecranText;

    // Stări Joc & Bani
    private float totalClientCurent = 0f;
    private float baniPrimitiDeLaClient = 0f;
    private float restNecesar = 0f;
    private float restOferitDeJucator = 0f;

    private float scorTotalJoc = 0f; // Adună doar clienții serviți corect
    private bool fazaScanare = true;
    private bool fazaRest = false;
    private bool gameOver = false;

    // Stări interne
    private List<GameObject> produsePeBanda = new List<GameObject>();
    private GameObject clientCurent;
    private bool clientLaCasa = false;
    private bool produseOprite = false;

    void Start()
    {
        GenereazaClientNou();
    }

    void Update()
    {
        if (gameOver) return;

        HandeMiscaClient();
        HandleMiscaProduse();

        if (fazaScanare)
        {
            HandleScanareProduse();
        }
        else if (fazaRest)
        {
            HandlePredareRest();
        }
    }

    public void GenereazaClientNou()
    {
        CurataBanda();

        // Resetare valori per client
        totalClientCurent = 0f;
        baniPrimitiDeLaClient = 0f;
        restNecesar = 0f;
        restOferitDeJucator = 0f;
        fazaScanare = true;
        fazaRest = false;

        int indexClient = Random.Range(0, prefabsClienti.Length);
        clientCurent = Instantiate(prefabsClienti[indexClient], punctPornireClient.position, punctPornireClient.rotation);
        clientLaCasa = false;
        produseOprite = false;

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

    void HandeMiscaClient()
    {
        if (clientCurent == null || punctOprireClient == null) return;

        if (Vector3.Distance(clientCurent.transform.position, punctOprireClient.position) > 0.1f)
        {
            clientCurent.transform.position = Vector3.MoveTowards(
                clientCurent.transform.position,
                punctOprireClient.position,
                vitezaClient * Time.deltaTime
            );
        }
        else
        {
            clientLaCasa = true;
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
        // Scanăm doar dacă SUNTEM în faza de scanare și NE UITĂM la bandă (sau apăsăm E scurt)
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (produseOprite && produsePeBanda.Count > 0 && fazaScanare)
            {
                GameObject produsScanat = produsePeBanda[0];
                Produs infoProdus = produsScanat.GetComponent<Produs>();

                string nume = "Produs";
                float pret = 1.0f;

                if (infoProdus != null)
                {
                    nume = infoProdus.numeProdus;
                    pret = infoProdus.pret;
                }

                totalClientCurent += pret;
                produsePeBanda.RemoveAt(0);
                Destroy(produsScanat);

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

    void IncepeFazaRest()
    {
        fazaScanare = false;
        fazaRest = true;

        // Clientul dă o bancnotă mai mare decât totalul (rotunjit la 5 sau 10 lei în sus)
        if (totalClientCurent <= 5f) baniPrimitiDeLaClient = 5f;
        else if (totalClientCurent <= 10f) baniPrimitiDeLaClient = 10f;
        else if (totalClientCurent <= 20f) baniPrimitiDeLaClient = 20f;
        else baniPrimitiDeLaClient = Mathf.Ceil(totalClientCurent / 10f) * 10f + 10f;

        restNecesar = baniPrimitiDeLaClient - totalClientCurent;

        AfiseazaStareRest();
    }

    // Apeleat din scriptul Bancnota.cs când apeși click pe o bancnotă
    public void AdaugaRest(float valoare)
    {
        if (!fazaRest || gameOver) return;

        restOferitDeJucator += valoare;
        AfiseazaStareRest();
    }

    void HandlePredareRest()
    {
        // Apasă Q pentru a confirma restul dat
        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            if (Mathf.Approximately(restOferitDeJucator, restNecesar) || Mathf.Abs(restOferitDeJucator - restNecesar) < 0.01f)
            {
                // REST CORECT!
                scorTotalJoc += totalClientCurent; // Adăugăm la scorul jocului
                ActualizeazaEcran("<color=green>REST CORECT!</color>\nVine alt client...");

                fazaRest = false;
                Destroy(clientCurent);
                Invoke("GenereazaClientNou", 2.0f);
            }
            else
            {
                // REST GREȘIT -> GAME OVER
                gameOver = true;
                fazaRest = false;
                ActualizeazaEcran($"<color=red>REST GRESIT!</color>\nClientul a primit {restOferitDeJucator} LEI\nTrebuia: {restNecesar} LEI\n\n<b>GAME OVER!</b>\nSCOR FINAL: {scorTotalJoc:F2} LEI");
            }
        }
    }

    void AfiseazaStareRest()
    {
        ActualizeazaEcran($"TOTAL CLIENT: {totalClientCurent} LEI\nPRIMII: {baniPrimitiDeLaClient} LEI\nREST DE DAT: {restNecesar} LEI\n----------\nREST ALES: {restOferitDeJucator} LEI\n(Apasa Q pt confirmare)");
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
        if (clientCurent != null) Destroy(clientCurent);
    }
}