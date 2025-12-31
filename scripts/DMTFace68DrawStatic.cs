using DlibFaceLandmarkDetector;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;

using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.SocialPlatforms.GameCenter;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.UIElements;

public class DMTFace68DrawStatic : MonoBehaviour
{
    private Texture2D debugTexture;
    private Renderer debugRenderer; // to 3D object
    private Mat debugMatrix;

    public GameObject drawFace;
    private RawImage debugRawImage; // to UI

    // ##########################################################################
    // ## MAIN Face Detection 
    // ##########################################################################

    /* points overview:
        0...16  LeftEar-Chin-RightEar
        17...21 LeftEyeBrowLR
        22...26 RightEyeBrowLR
        27...30 NoseTop-NoseTip
        31...35 NoseBottomLR

        36...39 LeftEyeTopLR
        40...41 LeftEyeBottomRL
        42...45 RightEyeTopLR
        46...47 RightEyeBottomRL

        48...54 OuterMouthTopLR
        55...59 OuterMouthBottomRL
        60...64 InnerMouthTopLR
        65...67 InnerMouthTopRL
    */


    IEnumerator Start()
    {

        Debug.Log("DMTFace68DrawStatic::Start and waits for Web-CAM init");
        // yield return new WaitUntil(() => DMT.DMTStaticStoreFace68.faceCounter >= 0);

        while ((DMT.DMTStaticStoreFace68.faceCounter < 0) || (DMT.DMTStaticStoreFace68.imgHeight == 0) || (DMT.DMTStaticStoreFace68.imgWidth == 0)) // wait till cam init and image size known
        {
            yield return null; // ein Frame warten
        }

        // yield return new WaitForSeconds(3.0f); // wait for DMT init
        Debug.LogWarning("DMTFace68DrawStatic::Start Image>" + DMT.DMTStaticStoreFace68.imgWidth + "x" + DMT.DMTStaticStoreFace68.imgHeight);

        debugMatrix = new Mat(DMT.DMTStaticStoreFace68.imgHeight, DMT.DMTStaticStoreFace68.imgWidth, CvType.CV_8UC4);
        debugTexture = new Texture2D(DMT.DMTStaticStoreFace68.imgWidth, DMT.DMTStaticStoreFace68.imgHeight, TextureFormat.RGBA32, false);
        if (debugRenderer != null) debugRenderer.GetComponent<Renderer>().material.mainTexture = debugTexture;

        debugRawImage = drawFace.GetComponent<RawImage>();
        debugRawImage.texture = debugTexture;
    }

    IEnumerator Sleep(int waitSec)
    {
        Debug.Log("Pause");
        yield return new WaitForSeconds(waitSec);
    }

    void Update()
    {
        if (DMT.DMTStaticStoreFace68.faceDetected && (debugTexture != null))
        {
            drawFace.SetActive(true);
            debugMatrix = new Mat(DMT.DMTStaticStoreFace68.imgHeight, DMT.DMTStaticStoreFace68.imgWidth, CvType.CV_8UC4, new Scalar(0, 0, 0, 0));

            FaceDraw();

            Point nosePoint = DMT.DMTStaticStoreFace68.nosePoint;
            // Debug.Log("Nose Point: " + nosePoint);
            Imgproc.circle(debugMatrix, nosePoint, 5, new Scalar(255, 0, 0, 255), -1);

            // Draw Bounding Box
            UnityEngine.Rect drawBB = DMT.DMTStaticStoreFace68.myBoundingBox;
            Imgproc.rectangle(debugMatrix, new Point(drawBB.xMin, drawBB.yMin), new Point(drawBB.xMax, drawBB.yMax), new Scalar(255, 0, 0, 255), 2);
            Imgproc.line(debugMatrix, new Point(drawBB.xMin, drawBB.yMin), new Point(drawBB.xMax, drawBB.yMax), new Scalar(255, 255, 0, 255), 1);
            Imgproc.line(debugMatrix, new Point(drawBB.xMax, drawBB.yMin), new Point(drawBB.xMin, drawBB.yMax), new Scalar(255, 255, 0, 255), 1);

            OpenCVForUnity.UnityIntegration.OpenCVMatUtils.MatToTexture2D(debugMatrix, debugTexture);
            debugRawImage.texture = debugTexture;
        }
        else
            drawFace.SetActive(false);

    }

    void FaceDraw()
    {
        Point oldPoint = new Point(0, 0);
        Point newPoint = new Point(0, 0);
        int run = 0;

        List<Vector2> points = DMT.DMTStaticStoreFace68.faceLandmark;

        for (run = 0; run < points.Count; run++) // lines
        {
            newPoint = new Point(points[run].x, points[run].y);

            if ((run != 0) && (run != 17) && (run != 22) && (run != 27) && (run != 36) && (run != 42) && (run != 48))
            {
                // OpenCVForUnity.ImgprocModule.Imgproc
                Imgproc.line(debugMatrix, oldPoint, newPoint, new Scalar(0, 0, 255, 255), 2);
            }

            if (run ==35) Imgproc.line(debugMatrix, new Point(points[30].x, points[30].y), newPoint, new Scalar(0, 0, 255, 255), 2);
            if (run == 41) Imgproc.line(debugMatrix, new Point(points[36].x, points[36].y), newPoint, new Scalar(0, 0, 255, 255), 2);
            if (run == 47) Imgproc.line(debugMatrix, new Point(points[42].x, points[42].y), newPoint, new Scalar(0, 0, 255, 255), 2);
            if (run == 67) Imgproc.line(debugMatrix, new Point(points[60].x, points[60].y), newPoint, new Scalar(0, 0, 255, 255), 2);

            Imgproc.circle(debugMatrix, newPoint,  3, new Scalar(0, 255, 0, 255), -1);

            oldPoint = newPoint;
        }
    }

}
