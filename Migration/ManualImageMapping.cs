using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Migration
{
    public class ManualImageMapping
    {
        private readonly FirestoreDb _db;

        // Manual mapping of names to image URLs from Google Sites
        // These URLs need to be collected manually from the Google Sites
        private readonly Dictionary<string, string> _imageUrls = new Dictionary<string, string>
        {
            // Staff Hall of Fame
            { "ROBERT SHAFER", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "BEATRICE ECKLER RASK", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "CHARLES GUYDER", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "RIT MORENO", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "JOE BENA", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "BRIAN McGARRY", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "FRANK DeMASI", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "MURIEL CHATTERTON", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "HOWARD \"CAPPY\" SCHWORM", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "JOHN CONWAY", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },

            // Alumni Hall of Fame
            { "RUTH EASTON", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "MILDRED SCHWORM", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "JOYCE PUTTERMAN", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "WILLIAM BARNES", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "CHARLES WILBUR", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "WILLIAM DEFOREST", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "BRUCE PANAS", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "JAMES BREITENSTEIN", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "FREDERICK DYKEMAN", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "LOIS CHATTERTON-ROGERS-WATSON", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "SANDRA SCHLIM", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "JUDITH ASH BROCKE", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "RAYMOND HAWES", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "RICHARD MURRAY", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "MARK CHATTERTON", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "ALFRED MILLER", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "MARTIN WILLIAMS", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "STEVEN SCHRADE", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "DAVID VINCENT", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "TIMOTHY GILCHRIEST", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "LARRY KEMMER", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "JOSEPH MERLI", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "LOIS MILLER", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "BRUCE EVANS", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "BARBARA SALISBURG", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "DAVID HEISIG", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "WENDY GRAVES", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "LYLA MEADER", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "JENNIFER WOLFE", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "AMY CHRISTMAN", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "MARY TERRELL", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "CHRISTOPHER MURRAY", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "STEPHEN J. DUBNER", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "STEVEN SCRANTON", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "DONALD LARGETEAU", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "SHARON LEE CROSIER SMITH", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "JOHN TELOW", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "BARTON MACDOUGALL", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "NICK GWIAZDOWSKI", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" },
            { "AMY (WHITBECK) GOLDING", "https://lh5.googleusercontent.com/1bLaYJ2Ht4CzFPOSGjAUpoxscoOCm-U33d3kT0KTXmEV7_wyrQErHyBDc0LbGn4WPgW3N4uZgAqMoByXX7jON_lw7k-avAzY58p_tRBIrhsOIRoO92bPSvS9twiwY3JpdTXH10VKGVgagcQUznnHb2PtzkJ0fXTmOjHocAscC26TEVEL7IsC3g=w1280" }
        };

        public ManualImageMapping()
        {
            // Initialize Firestore
            var credentialsPath = Path.Combine(Directory.GetCurrentDirectory(), "firebase-credentials-dcshalloffame.json");
            var builder = new FirestoreDbBuilder
            {
                ProjectId = "dcshalloffame",
                CredentialsPath = credentialsPath
            };
            _db = builder.Build();
        }

        public async Task RunAsync()
        {
            Console.WriteLine("Starting manual image mapping...");

            try
            {
                await UpdateDatabaseWithImages();
                Console.WriteLine("Image mapping completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during image mapping: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        private async Task UpdateDatabaseWithImages()
        {
            Console.WriteLine("Updating database with image URLs...");

            var collection = _db.Collection("hallOfFameMembers");
            var snapshot = await collection.GetSnapshotAsync();

            int updatedCount = 0;
            int totalCount = 0;

            foreach (var document in snapshot.Documents)
            {
                totalCount++;
                var data = document.ConvertTo<Dictionary<string, object>>();

                if (data.ContainsKey("name") && data["name"] is string name)
                {
                    // Case-insensitive match
                    var match = _imageUrls.FirstOrDefault(kvp => kvp.Key.Equals(name, StringComparison.OrdinalIgnoreCase));
                    if (!string.IsNullOrEmpty(match.Key))
                    {
                        var imageUrl = match.Value;

                        // Update the document with the image URL
                        var updates = new Dictionary<string, object>
                        {
                            { "imageUrl", imageUrl }
                        };

                        await document.Reference.UpdateAsync(updates);
                        Console.WriteLine($"Updated {name} with image URL");
                        updatedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"No image found for {name}");
                    }
                }
            }

            Console.WriteLine($"Updated {updatedCount} out of {totalCount} members with images");
        }
    }
}