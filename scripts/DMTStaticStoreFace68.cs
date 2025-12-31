
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using System;
using System.Collections.Generic;
using UnityEngine;

// usage: DMT.DMTStaticStoreFace68.myData = ...

namespace DMT
{
    public static class DMTStaticStoreFace68
    {
        // Image Infos
        private static int _imgWidth = 0;
        private static int _imgHeight = 0;

        // 68 Face Landmarks Points
        private static List<Vector2> _landmarks_screen; // = Face data.landmarks_screen;
        private static Vector2[] _landmarks_NDC;

        private static int _face_counter = -1; // number of detected faces, -1 no webcam 
        private static bool _face_detected = false;

        private static UnityEngine.Rect _boundingBox = UnityEngine.Rect.zero;
        private static UnityEngine.Rect _boundingBoxNDC = UnityEngine.Rect.zero;

        // special
        private static Point _nosePoint = new Point(0, 0);
        private static Vector2 _nosePointNDC = new Vector2(0, 0);

        private static double _faceAngleEye = 0.0f;
        private static double _faceAngleCenter = 0.0f;

        private static float _mouthDistance = 0.0f;
        private static float _mouthDistanceNDC = 0.0f;

        private static float _headLeftRightEar = 0.0f;
        private static float _headLeftRight = 0.0f;

        private static float _smileyFactor = 0.0f;

        // =====================================================================

        public static int imgWidth // image width
        {
            get { return _imgWidth; }
            set
            {
                _imgWidth = value;
            }
        }

        public static int imgHeight // image height
        {
            get { return _imgHeight; }
            set
            {
                _imgHeight = value;
            }
        }

        // =====================================================================
        //
        // FACE
        // 68 Face Landmarks Points in screen coordinates

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

        public static List<Vector2> faceLandmark // 68 Face Landmarks Points in screen coordinates
        {
            get { return _landmarks_screen; }
            set
            {
                _landmarks_screen = value;
                _landmarks_NDC = ndcLandmark();

                // update BoundingBox and BB-NDC
                _boundingBox = FindBoundingBox();
                _boundingBoxNDC = ConvertBoundingBox2NDC();
            }
        }

        private static Vector2[] ndcLandmark()
        {
            Vector2[] ndcPoints = new Vector2[_landmarks_screen.Count];
            for (int i = 0; i < _landmarks_screen.Count; i++)
            {
                float xNDC = _landmarks_screen[i].x / imgWidth;
                float yNDC = 1.0f - (_landmarks_screen[i].y / imgHeight); // flip Y axis

                ndcPoints[i] = new Vector2(xNDC, yNDC);
            }
            return ndcPoints;
        }

        public static int faceCounter // number of detected faces
        {
            get { return _face_counter; }
            set
            {
                _face_counter = value;
                _face_detected = (_face_counter > 0) ? true : false;
            }
        }

        public static bool faceDetected // is face detected
        {
            get { return _face_detected; }
        }

        // =====================================================================
        // =====================================================================

        public static Vector2 BoundingBoxCenter // main BoundingBoxes
        {
            get { return new Vector2(_boundingBox.x + _boundingBox.width / 2, _boundingBox.y + _boundingBox.height / 2); }
        }

        public static Vector2 BoundingBoxCenterNDC // main BoundingBoxes
        {
            get { return new Vector2(_boundingBoxNDC.x + _boundingBoxNDC.width / 2, _boundingBoxNDC.y + _boundingBoxNDC.height / 2); }
        }

        public static UnityEngine.Rect myBoundingBox // main BoundingBoxes
        {
            get { return _boundingBox; }
            set
            {
                UnityEngine.Rect gotData = value;
                _boundingBox = gotData;
            }
        }

        public static UnityEngine.Rect myBoundingBoxNDC // main BoundingBoxes
        {
            get { return _boundingBoxNDC; }
            set
            {
                UnityEngine.Rect gotData = value;
                _boundingBoxNDC = gotData;
            }
        }

        // .....................................................................
        // =====================================================================

        private static UnityEngine.Rect FindBoundingBox()
        {
            List<Vector2> points = _landmarks_screen;

            Point faceBBMin = new Point(imgWidth, imgHeight);
            Point faceBBMax = new Point(0, 0);

            foreach (Vector2 point in points)
            {
                if (point.x > faceBBMax.x) faceBBMax.x = point.x;
                if (point.y > faceBBMax.y) faceBBMax.y = point.y;
                if (point.x < faceBBMin.x) faceBBMin.x = point.x;
                if (point.y < faceBBMin.y) faceBBMin.y = point.y;
            }

            UnityEngine.Rect myBoundingBox =
                new UnityEngine.Rect((float)faceBBMin.x, (float)faceBBMin.y, (float)faceBBMax.x - (float)faceBBMin.x, (float)faceBBMax.y - (float)faceBBMin.y);

            return myBoundingBox;
        }

        private static UnityEngine.Rect ConvertBoundingBox2NDC()
        {
            UnityEngine.Rect bb = _boundingBox;

            float xNDC = bb.x / imgWidth;
            float yNDC = 1.0f - ((bb.y + bb.height) / imgHeight); // flip Y axis, because bb.y is top-left corner and NDC y=0 is bottom
            float wNDC = bb.width / imgWidth;
            float hNDC = bb.height / imgHeight;

            UnityEngine.Rect myBoundingBoxNDC = new UnityEngine.Rect(xNDC, yNDC, wNDC, hNDC);

            return myBoundingBoxNDC;
        }

        // .....................................................................
        // =====================================================================
        // special calculations 

        public static Point nosePoint
        {
            get
            {
                if (_landmarks_screen != null && _face_detected)
                {
                    _nosePoint.x = _landmarks_screen[30].x;
                    _nosePoint.y = _landmarks_screen[30].y;
                }
                return _nosePoint;
            }
        }

        public static Vector2 nosePointNDC
        {
            get
            {
                if (_landmarks_NDC != null && _face_detected)
                {
                    _nosePointNDC = _landmarks_NDC[30];
                }
                return _nosePointNDC;
            }
        }

        public static double faceAngleEye
        {
            get
            {
                if (_landmarks_screen != null && _face_detected)
                {
                    Point leftPoint = new Point(_landmarks_screen[36].x, _landmarks_screen[36].y); // left eye corner
                    Point rightPoint = new Point(_landmarks_screen[45].x, _landmarks_screen[45].y); // right eye corner

                    double deltaY = rightPoint.y - leftPoint.y;
                    double deltaX = rightPoint.x - leftPoint.x;

                    _faceAngleEye = Math.Atan2(deltaY, deltaX) * 180.0f / Math.PI;
                }
                return _faceAngleEye;
            }
        }

        public static double faceAngleCenter
        {
            get
            {
                if (_landmarks_screen != null && _face_detected)
                {
                    Point centerPoint = new Point(_landmarks_screen[8].x, _landmarks_screen[8].y); // chin point
                    Point winkelPoint = new Point(_landmarks_screen[27].x, _landmarks_screen[27].y); // nose top point

                    _faceAngleCenter = Math.Atan2(winkelPoint.y - centerPoint.y, winkelPoint.x - centerPoint.x) * 180.0f / Math.PI + 90.0f;
                }
                return _faceAngleCenter;
            }
        }

        public static float mouthDistance
        {
            get
            {
                if (_landmarks_screen != null && _face_detected)
                {
                    Vector2 topPoint = _landmarks_screen[51];    // outer mouth top - 51-57 62-66
                    Vector2 bottomPoint = _landmarks_screen[57]; // outer mouth bottom

                    _mouthDistance = Vector2.Distance(topPoint, bottomPoint);
                }
                return _mouthDistance;
            }
        }   

        public static float mouthDistanceNDC
        {
            get
            {
                if (_landmarks_NDC != null && _face_detected)
                {
                    Vector2 topPoint = _landmarks_NDC[51];    // outer mouth top - 51-57 62-66
                    Vector2 bottomPoint = _landmarks_NDC[57]; // outer mouth bottom

                    _mouthDistanceNDC = Vector2.Distance(topPoint, bottomPoint);
                }
                return _mouthDistanceNDC;
            }
        }

        public static float headLeftRightEar // from ear to ear
        {
            get
            {
                if (_landmarks_screen != null && _face_detected)
                {
                    Vector2 leftPoint = _landmarks_screen[0];   // left ear
                    Vector2 rightPoint = _landmarks_screen[16]; // right ear
                    Vector2 nosePoint = _landmarks_screen[30];  // nose tip

                    float faceWidth = Vector2.Distance(leftPoint, rightPoint);
                    float noseToLeft = Vector2.Distance(nosePoint, leftPoint);

                    _headLeftRightEar = (noseToLeft / faceWidth) * 2.0f - 1.0f; // -1.0 (left) to 1.0 (right)
                }
                return _headLeftRightEar;
            }
        }

        public static float headLeftRight // from face side
        {
            get
            {
                if (_landmarks_screen != null && _face_detected)
                {
                    float leftDistanz = Vector2.Distance(_landmarks_screen[2], _landmarks_screen[30]); // left face side to nose
                    float rightDistanz = Vector2.Distance(_landmarks_screen[30], _landmarks_screen[14]); // right face side to nose

                    _headLeftRight= leftDistanz / (leftDistanz + rightDistanz) *2 -1 ; // -1.0 (left) to 1.0 (right)
                }
                return _headLeftRight;
            }
        }

        public static float smileyFactor // mouth smile factor
        {
            get
            {
                if (_landmarks_screen != null && _face_detected)
                {
                    Vector2 leftPoint = _landmarks_screen[48]; // left mouth corner
                    Vector2 rightPoint = _landmarks_screen[54]; // right mouth corner

                    Vector2 topPoint = _landmarks_screen[51]; // outer mouth top
                    Vector2 bottomPoint = _landmarks_screen[57]; // outer mouth bottom

                    float center = (topPoint.y + bottomPoint.y) / 2;
                    float smiley = (leftPoint.y + rightPoint.y) / 2;

                    _smileyFactor = (center - smiley) / imgHeight * 1000; // normalized value
                }
                return _smileyFactor;
            }
        }   

        // .....................................................................

    }
}



