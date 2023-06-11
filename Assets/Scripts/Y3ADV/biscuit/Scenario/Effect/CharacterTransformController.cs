using System;
//using biscuit.Scenario.Common;
using UnityEngine;
using Y3ADV;

namespace biscuit.Scenario.Effect
{
    // Token: 0x020002B2 RID: 690
    public class CharacterTransformController : MonoBehaviour
    {
        // Token: 0x1700044B RID: 1099
        // (get) Token: 0x06001656 RID: 5718 RVA: 0x00066558 File Offset: 0x00064758
        // (set) Token: 0x06001657 RID: 5719 RVA: 0x00066560 File Offset: 0x00064760
        public int CharacterID { get; set; }

        // Token: 0x1700044C RID: 1100
        // (get) Token: 0x06001658 RID: 5720 RVA: 0x0006656C File Offset: 0x0006476C
        // (set) Token: 0x06001659 RID: 5721 RVA: 0x00066574 File Offset: 0x00064774
        public long CharacterImageID { get; set; }

        // Token: 0x0600165A RID: 5722 RVA: 0x00066580 File Offset: 0x00064780
        private void Awake()
        {
            this.CharacterID = 0;
        }

        // Token: 0x0600165B RID: 5723 RVA: 0x0006658C File Offset: 0x0006478C
        public void Setup(int id)
        {
            this.CharacterID = id;
            this.ChangeColor();
            this.ChangeCharacter();
        }

        // Token: 0x0600165C RID: 5724 RVA: 0x000665A4 File Offset: 0x000647A4
        private void ChangeColor()
        {
            Color white = Color.white;
            int characterID = this.CharacterID;
            switch (characterID)
            {
                case 1:
                    white = this.effectColor_yy;
                    break;
                case 2:
                    white = this.effectColor_mt;
                    break;
                case 3:
                    white = this.effectColor_fi;
                    break;
                case 4:
                    white = this.effectColor_ii;
                    break;
                case 5:
                    white = this.effectColor_km;
                    break;
                case 6:
                    white = this.effectColor_sw;
                    break;
                case 7:
                    white = this.effectColor_gm;
                    break;
                case 8:
                    white = this.effectColor_sn;
                    break;
                case 9:
                    white = this.effectColor_wn;
                    break;
                case 10:
                    white = this.effectColor_td;
                    break;
                case 11:
                    white = this.effectColor_ai;
                    break;
                case 12:
                    white = this.effectColor_ck;
                    break;
                case 13:
                    white = this.effectColor_yt;
                    break;
                case 14:
                    white = this.effectColor_us;
                    break;
                case 15:
                    white = this.effectColor_sn2;
                    break;
                case 16:
                    white = this.effectColor_sa;
                    break;
                case 17:
                    white = this.effectColor_nk;
                    break;
                case 20:
                    white = this.effectColor_ya;
                    break;
                case 22:
                    white = this.effectColor_mk;
                    break;
                case 23:
                    white = this.effectColor_sk;
                    break;
                case 24:
                    white = this.effectColor_ym;
                    break;
                case 25:
                case 26:
                    white = this.effectColor_sy;
                    break;
                case 34:
                    white = this.effectColor_yf;
                    break;
                case 35:
                    white = this.effectColor_yz;
                    break;
                case 43:
                    white = this.effectColor_fi2;
                    break;
                default:
                    break;
            }
            this.colorChangeBg.color = white;
            var main = this.colorChangeParticle.main;
            main.startColor = white;
        }

        // Token: 0x0600165D RID: 5725 RVA: 0x00066720 File Offset: 0x00064920
        private void ChangeCharacter()
        {
            if (this.CharacterID == 0)
            {
                return;
            }

            StartCoroutine(TransformPrefabCommand.LoadTransSprite(CharacterID, texture2D =>
            {
                characterImage.sprite = Sprite.Create(texture2D, 
                    new Rect(0, 0, texture2D.width, texture2D.height),
                    Vector2.one * 0.5f,
                    100.0f,  0, SpriteMeshType.FullRect);
                characterImage.sprite.name = $"trans_{CharacterID}";
            }));
        }

        // Token: 0x04001376 RID: 4982
        public Action CallBack;

        // Token: 0x04001377 RID: 4983
        [SerializeField]
        private SpriteRenderer characterImage;

        // Token: 0x04001378 RID: 4984
        [SerializeField]
        private SpriteRenderer colorChangeBg;

        // Token: 0x04001379 RID: 4985
        [SerializeField]
        private ParticleSystem colorChangeParticle;

        // Token: 0x0400137A RID: 4986
        [SerializeField]
        [Header("結城 友奈")]
        private Color effectColor_yy = Color.white;

        // Token: 0x0400137B RID: 4987
        [SerializeField]
        [Header("東郷 美森")]
        private Color effectColor_mt = Color.white;

        // Token: 0x0400137C RID: 4988
        [SerializeField]
        [Header("犬吠埼 風")]
        private Color effectColor_fi = Color.white;

        // Token: 0x0400137D RID: 4989
        [Header("犬吠埼 風 (眼帯)")]
        [SerializeField]
        private Color effectColor_fi2 = Color.white;

        // Token: 0x0400137E RID: 4990
        [Header("犬吠埼 樹")]
        [SerializeField]
        private Color effectColor_ii = Color.white;

        // Token: 0x0400137F RID: 4991
        [SerializeField]
        [Header("三好 夏凜")]
        private Color effectColor_km = Color.white;

        // Token: 0x04001380 RID: 4992
        [SerializeField]
        [Header("鷲尾 須美 (小学生)")]
        private Color effectColor_sw = Color.white;

        // Token: 0x04001381 RID: 4993
        [Header("三ノ輪 銀 (小学生)")]
        [SerializeField]
        private Color effectColor_gm = Color.white;

        // Token: 0x04001382 RID: 4994
        [SerializeField]
        [Header("乃木 園子 (小学生)")]
        private Color effectColor_sn = Color.white;

        // Token: 0x04001383 RID: 4995
        [SerializeField]
        [Header("乃木 園子 (中学生)")]
        private Color effectColor_sn2 = Color.white;

        // Token: 0x04001384 RID: 4996
        [SerializeField]
        [Header("乃木 若葉")]
        private Color effectColor_wn = Color.white;

        // Token: 0x04001385 RID: 4997
        [Header("土居 球子")]
        [SerializeField]
        private Color effectColor_td = Color.white;

        // Token: 0x04001386 RID: 4998
        [Header("伊予島 杏")]
        [SerializeField]
        private Color effectColor_ai = Color.white;

        // Token: 0x04001387 RID: 4999
        [Header("郡 千景")]
        [SerializeField]
        private Color effectColor_ck = Color.white;

        // Token: 0x04001388 RID: 5000
        [Header("高嶋 友奈")]
        [SerializeField]
        private Color effectColor_yt = Color.white;

        // Token: 0x04001389 RID: 5001
        [Header("白鳥 歌野")]
        [SerializeField]
        private Color effectColor_us = Color.white;

        // Token: 0x0400138A RID: 5002
        [SerializeField]
        [Header("秋原 雪花")]
        private Color effectColor_sa = Color.white;

        // Token: 0x0400138B RID: 5003
        [Header("古波蔵 棗")]
        [SerializeField]
        private Color effectColor_nk = Color.white;

        // Token: 0x0400138C RID: 5004
        [Header("赤嶺 友奈")]
        [SerializeField]
        private Color effectColor_ya = Color.white;
        [Header("楠 芽吹")]
        [SerializeField]
        private Color effectColor_mk = Color.white;
        [Header("加賀城 雀")]
        [SerializeField]
        private Color effectColor_sk = Color.white;
        [Header("弥勒 夕海子")]
        [SerializeField]
        private Color effectColor_ym = Color.white;
        [Header("山伏 しずく")]
        [SerializeField]
        private Color effectColor_sy = Color.white;
        [Header("芙蓉 友奈")]
        [SerializeField]
        private Color effectColor_yf = Color.white;
        [Header("柚木 友奈")]
        [SerializeField]
        private Color effectColor_yz = Color.white;
    }
}
