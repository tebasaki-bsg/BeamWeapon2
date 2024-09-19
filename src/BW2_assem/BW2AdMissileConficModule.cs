using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;
using Modding;
using Modding.Serialization;
using Modding.Modules;
using Modding.Blocks;
using skpCustomModule;

namespace BW2Space
{
    [XmlRoot("BW2AdMissileConfigModule")]
    [Reloadable]

    public class BW2AdMissileConfigModule : BlockModule
    {
        [XmlElement("GuideRatio")]
        public MSliderReference GuideSlider;
    }

    public class BW2AdMissileConfigBehaviour : BlockModuleBehaviour<BW2AdMissileConfigModule>
    {
        private MSlider guideslider;
        private float Guide;

        private Transform adprojectile;

        private AdShootingBehavour adshootingbehavour;
        private AdProjectileScript adprojectilescript;


        public override void OnSimulateStart()
        {
            base.OnSimulateStart();

            adshootingbehavour = base.GetComponent<AdShootingBehavour>();

            guideslider = GetSlider(Module.GuideSlider);
            Guide = guideslider.Value;

            if (!StatMaster.isMP)   //マルチでない→PHYSICS GOAL, マルチである→PManager→Projectile Pool
            {
                adprojectile = GameObject.Find("PHYSICS GOAL").transform;

                foreach (Transform child in adprojectile)
                {
                    adprojectilescript = child.gameObject.GetComponent<AdProjectileScript>();
                    if (adprojectilescript.BlockName == adshootingbehavour.BlockName)
                    {
                        adprojectilescript.mRatio = Guide;
                    }
                }
            }
            else
            {
                adprojectile = GameObject.Find("PManager").transform.Find("Projectile Pool");

                foreach (Transform child in adprojectile)
                {
                    adprojectilescript = child.gameObject.GetComponent<AdProjectileScript>();
                    if (adprojectilescript.BlockName == adshootingbehavour.BlockName)
                    {
                        adprojectilescript.mRatio = Guide;
                    }
                }
            }
        }

    }

}
