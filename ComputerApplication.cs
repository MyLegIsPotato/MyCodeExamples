using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Rozszerzenia;

[System.Serializable]
public class ComputerApplication //Klasa bazowa dla każdego programu PC
{
    protected string appName;
    protected bool isEnabled;
    protected bool showOnTaskBar;
    protected bool showInStart;
    protected float windowWidth;
    protected float windowHeight;

    protected Button[] topbarButtons;
    protected GameObject windowGO;
    protected GameObject titleGO;
    protected GameObject imageGO;
    protected GameObject bodyGO;
    protected GameObject activeIndicator;

    public GameObject windowPrefab;
    public GameObject startItemPrefab;
    public ComputerController pc;
    public Vector2 windowGoPos;
    public Vector2 defaultWindowSize;

    public bool minimized;
    public bool selected;
    public bool maximized;

    public string GetName()
    {
        return appName;
    }

    public ComputerApplication()
    {
        pc = ComputerController.instance;
        appName = "DefaultApp";
        isEnabled = false;
        showOnTaskBar = true;
        showInStart = true;
        windowWidth = 800;
        windowHeight = 900;
    }

    public ComputerApplication(string _appName)
    {
        pc = ComputerController.instance;
        appName = _appName;
        isEnabled = false;
        showOnTaskBar = true;
        showInStart = true;
        windowWidth = 800;
        windowHeight = 900;
    }

    public enum przyciskTopbar
    {
        ZAMKNIJ,
        POWIEKSZ,
        MINIMALIZUJ
    }


    public void DodajMetodeDoPrzycisku(przyciskTopbar rodzaj, Website_Bank.Walizeczka funkcja)
    {
        switch (rodzaj)
        {
            case przyciskTopbar.ZAMKNIJ:
                topbarButtons[0].onClick.AddListener(() => funkcja());
                break;
            case przyciskTopbar.POWIEKSZ:
                topbarButtons[1].onClick.AddListener(() => funkcja());
                break;
            case przyciskTopbar.MINIMALIZUJ:
                topbarButtons[2].onClick.AddListener(() => funkcja());
                break;
            default:
                break;
        }
    }

    public void DodajDoMenu()
    {
        if (showOnTaskBar)
        {
            GameObject taskbarButton = GameObject.Instantiate(pc.emptyRectTransform, ComputerController.instance.taskbar.transform);
            taskbarButton.name = "PC_Taskbar_" + appName;
            Button button = taskbarButton.AddComponent<Button>();
            Image buttonImage = taskbarButton.AddComponent<Image>();
            buttonImage.color = new Color32(255, 255, 255, 255);
            GameObject icon = GameObject.Instantiate(pc.emptyRectTransform, taskbarButton.transform);
            activeIndicator = GameObject.Instantiate(pc.emptyRectTransform, taskbarButton.transform);
            RectTransform indicator = activeIndicator.GetComponent<RectTransform>();
            indicator.anchorMin = new Vector2(.5f, 0);
            indicator.anchorMax = new Vector2(.5f, 0);
            indicator.sizeDelta = new Vector2(90, 5);
            Image indicatorImg = activeIndicator.AddComponent<Image>();
            indicatorImg.color = new Color(255, 255, 255, 126);
            RectTransform rt = icon.GetComponent<RectTransform>();
            Image img = icon.AddComponent<Image>();
            img.sprite = imageGO.GetComponent<Image>().sprite;
            button.targetGraphic = buttonImage.gameObject.GetComponent<Image>();
            button.UstawKoloryPrzycisku();
            button.onClick.AddListener(() => { PrzelaczMinimalizuj(); });
            button.onClick.AddListener(() => { Uruchom(); });
            activeIndicator.SetActive(false);
            //button.onClick.AddListener(() => { Util.PrzelaczWlaczone(windowGO); });
        }

        if (showInStart)
        {
            GameObject rightStartMenuBar = Util.WyszukajPoNazwieW(pc.startMenu.transform, "PC_StartMenu_RightBar").gameObject;
            GameObject startMenuItem = Util.InstantiateWithName(pc.emptyRectTransform, rightStartMenuBar.transform, "PC_StartMenuProgram_" + appName);
            RectTransform rt = startMenuItem.GetComponent<RectTransform>();
            Button bt = startMenuItem.AddComponent<Button>();
            Image background = startMenuItem.AddComponent<Image>();
            bt.targetGraphic = background;
            bt.UstawKoloryPrzycisku();
            GameObject startImageGO = Util.InstantiateWithName(pc.emptyRectTransform, startMenuItem.transform, "PC_Image");
            RectTransform startImageRT = startImageGO.GetComponent<RectTransform>();
            startImageRT.anchorMin = new Vector2(0, 0.5f);
            startImageRT.anchorMax = new Vector2(0, 0.5f);
            startImageRT.localPosition = Vector3.zero;
            startImageGO.transform.localScale = new Vector3(.7f, .7f, .7f);
            GameObject textGO = Util.InstantiateWithName(pc.emptyRectTransform, startMenuItem.transform, "PC_Text");
            Image icon = startImageGO.AddComponent<Image>();
            Text text = textGO.AddComponent<Text>();
            RectTransform textRT = textGO.GetComponent<RectTransform>();
            textRT.sizeDelta = rt.sizeDelta;
            textRT.anchorMin = new Vector2(0, 0);
            textRT.anchorMax = new Vector2(1, 1);
            textRT.offsetMin = new Vector2(100, 0);
            textRT.offsetMax = new Vector2(-25, 0);
            icon.sprite = imageGO.GetComponent<Image>().sprite;
            text.text = appName;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 20;
            text.font = pc.systemFont;
            text.material = pc.systemFont.material;
            text.color = pc.colorPreset.secondaryColor;
        }
    }

    public void Powieksz()
    {
        if (!maximized) //powieksz
        {
            maximized = true;
            pc.console.Wypisz("Powiekszam " + appName);
            defaultWindowSize = windowGO.GetComponent<RectTransform>().sizeDelta;
            windowGO.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
            windowGO.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
            windowGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            windowGO.GetComponent<RectTransform>().sizeDelta = new Vector2(1920, -pc.taskbar.GetComponent<RectTransform>().rect.height);
        }
        else //pomninejsz
        {
            maximized = false;
            windowGO.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
            windowGO.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
            windowGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(120, -120);
            windowGO.GetComponent<RectTransform>().sizeDelta = defaultWindowSize;
        }
    }

    public void Uruchom()
    {
        if (windowGO.activeSelf == false)
        {
            pc.console.Wypisz("Uruchamiam " + appName);
            windowGO.SetActive(true);
            selected = true;
            minimized = false;
            windowGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(120, -120);
            activeIndicator.SetActive(true);
            WyciagnijNaWierzch();
        }
    }

    public void Zamknij()
    {
        if (windowGO.activeSelf == true)
        {
            pc.console.Wypisz("Zamykam " + appName);
            windowGO.SetActive(false);
            selected = false;
            minimized = false;
            activeIndicator.SetActive(false);
        }
    }

    public void Minimalizuj()
    {
        pc.console.Wypisz("Minimalizuje " + appName);
        minimized = true;
        windowGoPos = windowGO.GetComponent<RectTransform>().localPosition;
        LeanTween.moveLocalY(windowGO, -1080, 0.3f);
    }

    public void WyciagnijNaWierzch()
    {
        windowGO.transform.SetSiblingIndex(pc.desktop.transform.childCount - 1);
    }

    public void PrzelaczMinimalizuj()
    {
        if (!minimized && selected) //Jesli widoczne i aktywne, to schowaj
        {
            minimized = true;
            windowGoPos = windowGO.GetComponent<RectTransform>().localPosition;
            LeanTween.moveLocalY(windowGO, -1080, 0.3f);
        }
        else if(windowGO.activeSelf)
        {
            minimized = false;
            LeanTween.moveLocalY(windowGO, windowGoPos.y, 0.3f);
        }

    }

    protected void StworzOknoDomyslne() //Tworzy okno domyslne
    {
        windowGO = MonoBehaviour.Instantiate(pc.appWindowPrefab, pc.desktop.GetComponent<RectTransform>().transform);
        EventTrigger eventTrigger = windowGO.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener((data) => { WyciagnijNaWierzch(); });
        eventTrigger.triggers.Add(entry);
        GameObject topbar = Util.WyszukajJedenPoTaguWewnatrz(windowGO.transform, "PC_Window_TopBar");
        topbarButtons = topbar.GetComponentsInChildren<Button>();
        windowGO.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
        windowGO.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
        windowGO.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
        windowGO.name = "PC_" + appName;

        topbarButtons[0].onClick.AddListener(() => { Zamknij(); });
        topbarButtons[1].onClick.AddListener(() => { Powieksz(); });
        topbarButtons[2].onClick.AddListener(() => { Minimalizuj(); });
        topbar.AddComponent<MouseDetector>().window = windowGO;
        topbar.GetComponent<BoxCollider2D>().size = new Vector2(windowWidth, 50);
        topbar.GetComponent<BoxCollider2D>().offset = new Vector2(0, -50/2);
        imageGO = Util.WyszukajJedenPoTaguWewnatrz(windowGO.transform, "PC_Window_AppImage");
        bodyGO = Util.WyszukajJedenPoTaguWewnatrz(windowGO.transform, "PC_Window_Body");
        titleGO = Util.WyszukajJedenPoTaguWewnatrz(windowGO.transform, "PC_WindowName");
        windowGO.SetActive(false);
    }

    public void LadujIkone(string path)
    {
        Sprite loaded = Resources.Load<Sprite>(path);
        imageGO.GetComponent<Image>().sprite = loaded;
    }
}

public class ComputerBrowser : ComputerApplication //Tu będzie przeglądarka
{
    string path = "ProgramyPC/PC_Browser/icon";
    Button addTab;
    GameObject hompageLinksContainer;
    private GameObject pagesContainer;

    public ComputerBrowser() : base(_appName: "Hydrodog Internet Browser")
    {
        //Ustaw obiekt do klasy bazowej:
        StworzOknoDomyslne();

        //Ustawienia dla tej aplikacji:
        titleGO.GetComponent<Text>().text = base.appName;
        windowGO.GetComponent<RectTransform>().sizeDelta = new Vector2(1400, 800);

        
        GameObject browser = MonoBehaviour.Instantiate(pc.browserWindowPrefab, bodyGO.transform);
        
        pc.console.Wypisz(appName + " has started...");
      
        LadujIkone(path);
        DodajDoMenu();

        hompageLinksContainer = Util.WyszukajPoNazwieW(windowGO.transform, "UI_Links_Container").gameObject;
        pagesContainer = Util.WyszukajPoNazwieW(windowGO.transform, "UI_Container_Pages").gameObject;
        //Debug.LogWarning(pagesContainer.name);
        foreach (KeyValuePair<string, GameObject> para in pc.strony)
        {
            if (para.Key != "-StronaStartowa")
            {
                GameObject temp = GameObject.Instantiate(pc.emptyRectTransform, hompageLinksContainer.transform);
                temp.name = "PC_Link_" + para.Value;
                temp.AddComponent<Image>().sprite = Resources.Load<Sprite>("ProgramyPC/PC_Browser/WebsitesIcons/" + para.Key); //Klucz musi być taki sam jak nazwa pliku .png w folderze!
                Button bt = temp.AddComponent<Button>();
                bt.onClick.AddListener(() => { DodajKarte(para, temp.GetComponent<Image>().sprite); });
            }
            else
            {
                DodajKarteStartowa(pc.prefabKarty, null);
            }
        }

        //browser.GetComponentInChildren<Website_Bank>().application = this;
    }

    public void WyswietlStrone(GameObject strona)
    {
        strona.transform.SetSiblingIndex(pagesContainer.transform.childCount);
    }

    public void DodajKarte(KeyValuePair<string, GameObject> para, Sprite sprite)
    {
        GameObject strona = GameObject.Instantiate(para.Value, Util.WyszukajPoNazwieW(windowGO.transform, "UI_Container_Pages"));
        GameObject karta = GameObject.Instantiate(pc.prefabKarty, Util.WyszukajPoNazwieW(windowGO.transform, "UI_Container_OpenTabs"));
        karta.GetComponentInChildren<Text>().text = para.Key;
        karta.GetComponent<Button>().onClick.AddListener(() => { WyswietlStrone(strona); });
        GameObject favicon = Util.WyszukajPoNazwieW(karta.transform, "UI_Favicon").gameObject;
        favicon.GetComponent<Image>().sprite = sprite;
        favicon.GetComponent<RectTransform>().localScale = new Vector2(.6f, .6f);
    }

    public void DodajKarteStartowa(GameObject prefab, Sprite sprite)
    {
        GameObject karta = GameObject.Instantiate(pc.prefabKarty, Util.WyszukajPoNazwieW(windowGO.transform, "UI_Container_OpenTabs"));
        GameObject strona = Util.WyszukajPoNazwieW(windowGO.transform, "UI_Page_Blank").gameObject;
        karta.GetComponentInChildren<Text>().text = "Home Page";
        karta.GetComponent<Button>().onClick.AddListener(() => { WyswietlStrone(strona); });
    }
}

public class ComputerConsole : ComputerApplication //Tu jest konsola
{
    string path = "ProgramyPC/PC_Console/icon";
    GridLayoutGroup gridLayout;
    public ComputerConsole() : base(_appName: "OpenDoors Debug Console")
    {
        //Ustaw obiekt do klasy bazowej:
        StworzOknoDomyslne();

        //Ustawienia dla tej aplikacji:
        bodyGO.GetComponent<Image>().color = new Color32(10, 10, 10, 255);
        gridLayout = bodyGO.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(base.windowWidth - 15, 30);
        gridLayout.padding.top = 15;
        gridLayout.padding.left = 10;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 1;
        titleGO.GetComponent<Text>().text = base.appName;
        windowGO.GetComponent<RectTransform>().sizeDelta = new Vector2(base.windowWidth, 500);

        LadujIkone(path);
        DodajDoMenu();

        WypiszStart();
        Wypisz("Rozmiar tego okna to: " + windowGO.GetComponent<RectTransform>().rect.height + " x " + windowGO.GetComponent<RectTransform>().rect.width);
        ComputerController.instance.console = this;
    }

    public void WypiszStart()
    {
        GameObject textRow = MonoBehaviour.Instantiate(pc.emptyRectTransform, bodyGO.transform);
        RectTransform row = textRow.GetComponent<RectTransform>();
        row.sizeDelta = new Vector2(base.windowWidth - 15, 30);
        row.anchorMin = new Vector2(0, 0.6f);
        row.anchorMax = new Vector2(0, 0.6f);
        row.anchoredPosition = new Vector2(base.windowWidth/2, 0);
        Text uiText = textRow.AddComponent<Text>();
        uiText.text = System.DateTime.Now.ToString() + " [---] " + "Initializing ingame console...";
        uiText.font = pc.consoleFont;
        uiText.color = new Color32(0, 220, 0, 255);
        uiText.fontSize = 21;
    }

    public void Wypisz(string text)
    {
        GameObject textRow = MonoBehaviour.Instantiate(pc.emptyRectTransform, bodyGO.transform);
        RectTransform row = textRow.GetComponent<RectTransform>();
        row.sizeDelta = new Vector2(base.windowWidth - 15, 30);
        row.anchorMin = new Vector2(0, 0.6f);
        row.anchorMax = new Vector2(0, 0.6f);
        row.anchoredPosition = new Vector2(base.windowWidth / 2, 0);
        Text uiText = textRow.AddComponent<Text>();
        uiText.text = System.DateTime.Now.ToString() + " [---] " + text;
        uiText.font = ComputerController.instance.consoleFont;
        uiText.color = new Color32(0, 220, 0, 255);
        uiText.fontSize = 21;
        //(body height - some margin) - row*rowCount 
        if(gridLayout.gameObject.transform.childCount * 30 + 60 >= windowGO.GetComponent<RectTransform>().rect.height)
        {
            gridLayout.padding.top = gridLayout.padding.top - 30;
        }

    }
}

public class ComputerSettings : ComputerApplication //Tu jest okno ustawień
{
}