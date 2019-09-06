using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Effects;
using Jypeli.Widgets;

/// @author Kristian Leirimaa
/// 10122015
/// <summary>
/// Luodaan SquirrelGlide fysiikkapeli
/// </summary>
public class SquirrelGlide : PhysicsGame
{
    private PhysicsObject orava;

    private IntMeter pisteLaskuri;
    private double scrollausnopeus = -0.5;
    
    private List<GameObject> taustakuvat;
    private Timer taustaAjastin = new Timer();
    private GameObject ekaTaustakuva;
    
    private Timer lisaaKapyja;
    private Timer lisaaPollo;

    private Image oravaKuva = LoadImage("HTorava");
    private Image polloKuva = LoadImage("HTpollo");

    /// <summary>
    /// Kutsutaan aliohjelmia
    /// </summary>
    public override void Begin()
    {
        LuoKenttä();
        LisaaNappaimet();
        LuoTaustakuvat();
        LisaaBoostit();
        LisaaPollo();
        KaikkiBoostit();
        LuoPistelaskuri();

    }
    /// <summary>
    /// Liikutetaan taustaa tasaisella nopeudella
    /// ja lisätään taustakuvat listaan
    /// </summary>
    void LuoTaustakuvat()
    {

        taustaAjastin = new Timer();
        taustaAjastin.Interval = 0.0025;   // tällä voit myös säätää nopeutta
        taustaAjastin.Timeout += LiikutaTaustaa;
        taustaAjastin.Start();

        taustakuvat = new List<GameObject>();
        LisaaTaustakuva("tausta", Screen.Width, Screen.Height);
        LisaaTaustakuva("tausta2", Screen.Width, Screen.Height);

    }

    /// <summary>
    /// tehdään taustakuvista olioita ja lisätään
    /// ne vuorotellen toistensa perään
    /// </summary>
    /// <param name="nimi"></param>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>
    void LisaaTaustakuva(string nimi, double leveys, double korkeus)
    {
        GameObject olio = new GameObject(leveys, korkeus);
        olio.Image = LoadImage(nimi);
        olio.X = 0;
        Add(olio, -3);

        if (taustakuvat.Count > 0)
        {
            olio.Left = taustakuvat[taustakuvat.Count - 1].Right;
            if (scrollausnopeus >= 0) ekaTaustakuva = olio;
        }
        else
        {
            olio.Left = Screen.Left;
            if (scrollausnopeus < 0) ekaTaustakuva = olio;
        }

        taustakuvat.Add(olio);
        Layers[-3].RelativeTransition = new Vector(0.5, 0.5);
    }

    /// <summary>
    /// liikutetaan taustaa
    /// </summary>
    void LiikutaTaustaa()
    {
        for (int i = 0; i < taustakuvat.Count; i++)
        {
            GameObject taustakuva = taustakuvat[i];
            taustakuva.X += scrollausnopeus;

            if (taustakuva.Right < Screen.Right)
            {
                GameObject edellinenTausta = i - 1 < 0 ? taustakuvat[taustakuvat.Count - 1] : taustakuvat[i - 1];
                edellinenTausta.Left = taustakuva.Right;

            }
        }
    }

    /// <summary>
    /// Luodaan kenttä ja lisätään orava sinne
    /// </summary>
    public void LuoKenttä()
    {
        Vector oravaPaikka = new Vector(Level.Left + 30.0, 50.0);

        orava = LuoOrava(oravaPaikka);

        Timer.SingleShot(1.0, AktivoiGravi);

        Gravity = new Vector(0, -700);

        Surfaces reunat = Level.CreateBorders();
        reunat.Left.IsVisible = false;
        reunat.Right.IsVisible = false;
        reunat.Left.X = Screen.Left;

        AddCollisionHandler(reunat.Left, "kapy", Poista);
        AddCollisionHandler(reunat.Bottom, "orava", Tormays);
        AddCollisionHandler(reunat.Left, "pollo", Poista);

        Camera.ZoomToLevel();
    }

    /// <summary>
    /// lisättävät boostit ja niiden sijainnit
    /// </summary>
    void KaikkiBoostit()
    {
        LuoBoostit(RandomGen.NextVector(Level.Right + 50, Level.Bottom + 50, Screen.Right - 50, Level.Top - 50));
        LuoBoostit(RandomGen.NextVector(Level.Right + 50, Level.Bottom + 50, Screen.Right - 50, Level.Top - 50));
        LuoBoostit(RandomGen.NextVector(Level.Right + 50, Level.Bottom + 50, Screen.Right - 50, Level.Top - 50));
    }

    void PolloKutsu()
    {
        LuoPollo(RandomGen.NextVector(Level.Right + 50, Level.Bottom + 50, Screen.Right - 50, Level.Top - 50));
        LuoPollo(RandomGen.NextVector(Level.Right + 50, Level.Bottom + 50, Screen.Right - 50, Level.Top - 50));
        lisaaPollo.Interval = RandomGen.NextInt(1, 3);
    }

    /// <summary>
    /// Lisätään boosteja kentälle tietty määrä
    /// aina tietyin väliajoin
    /// </summary>
    void LisaaBoostit()
    {
        lisaaKapyja = new Timer();
        lisaaKapyja.Interval = 1.5;
        lisaaKapyja.Timeout += KaikkiBoostit;
        lisaaKapyja.Start();

    }

    /// <summary>
    /// kutsutaan LuoPollo aliohjelmaa ja lisätään
    /// pöllö kentälle tietyin väliajoin
    /// </summary>
    void LisaaPollo()
    {
        int aika = RandomGen.NextInt(1, 3);

        lisaaPollo = new Timer();
        lisaaPollo.Interval = aika;
        lisaaPollo.Timeout += PolloKutsu;
        lisaaPollo.Start();
    }

    /// <summary>
    /// pelin näppäimet
    /// </summary>
    void LisaaNappaimet()
    {

        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Up, ButtonState.Pressed, OravaHyppy, "hypi oravalla");

    }

    /// <summary>
    /// luodaan oliot tässä aliohjelmassa
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="muoto"></param>
    /// <param name="sijainti"></param>
    /// <param name="oliovari"></param>
    /// <param name="tagi"></param>
    /// <param name="nopeus"></param>
    /// <param name="painovoima"></param>
    /// <param name="tormaa"></param>
    /// <param name="kimmoisuus"></param>
    /// <returns></returns>
    PhysicsObject LuoOliot(int x, int y, Shape muoto, Vector sijainti, Color oliovari, string tagi, Vector nopeus, bool painovoima = true, bool tormaa = true, double kimmoisuus = 0.0)
    {
        PhysicsObject olio = new PhysicsObject(x, y);
        olio.Shape = muoto;
        olio.Position = sijainti;
        olio.Color = oliovari;
        olio.Tag = tagi;
        olio.Velocity =nopeus;
        olio.IgnoresGravity = painovoima;
        olio.IgnoresCollisionResponse = tormaa;
        olio.Restitution = kimmoisuus;

        Add(olio);

        return olio;
    }

    /// <summary>
    /// luodaan boosti
    /// </summary>
    /// <param name="SquirrelGlide"></param>
    /// <param name="paikka"></param>
    void LuoBoostit(Vector paikka)
    {

        PhysicsObject kapy = LuoOliot(20, 25, Shape.Circle, paikka, Color.Brown, "kapy", new Vector(-400.0, 0.0));

    }

    /// <summary>
    /// luodaan pelihahmo, eli orava
    /// </summary>
    /// <param name="sijainti"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    PhysicsObject LuoOrava(Vector paikka)
    {

        orava = LuoOliot(80, 80, Shape.Circle, paikka, Color.AshGray, "orava", new Vector(0, 0));
        orava.Image = oravaKuva;
        orava.CanRotate = false;
        AddCollisionHandler(orava, "kapy", Poista);
        AddCollisionHandler(orava, "pollo", Tormays);

        return orava;
    }

    /// <summary>
    /// luodaan pöllö, orava ei saa osua pöllöön
    /// </summary>
    /// <param name="SquirrelGlide"></param>
    /// <param name="paikka"></param>
    /// <returns></returns>
    void LuoPollo(Vector paikka)
    {

        PhysicsObject pollo = LuoOliot(120, 120, Shape.Rectangle, paikka, Color.LightGray, "pollo", new Vector(-600.0, 0.0));
        pollo.CanRotate = false;
        pollo.Image = polloKuva;

    }

    /// <summary>
    /// Luodaan pistelaskuri, jonka aloitus arvo on 
    /// nolla, minimiarvo on nolla ja maximi arvo on
    /// 50. Pistelaskuri laskee kerättyjen boostien määrän
    /// </summary>
    void LuoPistelaskuri()
    {
        pisteLaskuri = new IntMeter(0, 0, 10);

        Label pisteNaytto = new Label(130.0, 60.0);
        pisteNaytto.X = Level.Left - 200.0;
        pisteNaytto.Y = Level.Top - 50.0;
        pisteNaytto.TextColor = Color.Black;
        pisteNaytto.Color = Color.White;
        pisteNaytto.IntFormatString = "Pisteet: {0}";

        pisteNaytto.BindTo(pisteLaskuri);
        Add(pisteNaytto);
    }

    /// <summary>
    /// laitetaan orava hyppäämään kun painetaan nuolinäppäintä
    /// ylös
    /// </summary>
    /// <param name="hyppy"></param>
    void OravaHyppy()
    {
        Vector pomppu = new Vector(0.0, 250.0);
        orava.Hit(pomppu);

    }

    /// <summary>
    /// aktivoidaan oravan painovoima
    /// pelin alussa
    /// </summary>
    void AktivoiGravi()
    {
        orava.IgnoresGravity = false;
    }

    /// <summary>
    /// poistetaan boosti ja lisätään piste pistelaskuriin
    /// </summary>
    /// <param name="tormaaja"></param>
    /// <param name="kohde"></param>
    void Poista(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        if (tormaaja.Tag.Equals("orava"))
        {
            kohde.Destroy();
            pisteLaskuri.Value += 1;

            if (pisteLaskuri.Value >= 10) Tormays(tormaaja, kohde);
        }
        if (kohde.Tag.Equals("kapy")) kohde.Destroy();
        if (kohde.Tag.Equals("pollo")) kohde.Destroy();
    }

    /// <summary>
    /// poistetaan liikkuminen ja ilmoitetaan, että peli
    /// on hävitty
    /// </summary>
    /// <param name="tormaaja"></param>
    /// <param name="kohde"></param>
    void Tormays(PhysicsObject tormaaja, PhysicsObject kohde)
    {

        if (pisteLaskuri.Value >= 10) MessageDisplay.Add("Voitit!");
        else MessageDisplay.Add("Hävisit!");

        Keyboard.Disable(Key.Up);
        lisaaPollo.Stop();
        taustaAjastin.Stop();
        lisaaKapyja.Stop();
        pisteLaskuri.Stop();
    }

}
