﻿using System;
using System.Collections.Generic;
using System.Linq;
using static System.Math;
using static System.Diagnostics.Debug;

using GIDOO_space;

namespace GNPX_space{
	using pGPGC = GNPX_Puzzle_Global_Control;
    public class GPuzzleTrans{
        private int[,]   prmX={ {0,1,2}, {1,2,0}, {2,0,1}, {2,1,0}, {0,2,1}, {1,0,2} };
        private int[,]   trsX={ {2,0,1,4,5,3}, {2,0,1,4,5,3}, {5,3,4,1,2,0},
                                {1,2,0,5,3,4}, {1,2,0,5,3,4}, {4,5,3,2,0,1} };
        private int[]    prmR={ 0,2,1, 3,5,4 };
        private string[] prmXst={"", "(123)->(312)", "(123)->(231)", "1-3 exchange", "2-3 exchange", "1-2 exchange",
                                 "", "(456)->(645)", "(456)->(564)", "4-6 exchange", "5-6 exchange", "4-5 exchange",
                                 "", "(789)->(978)", "(789)->(897)", "7-9 exchange", "8-9 exchange", "7-8 exchange"};

        private GNPX_App_Man    pGNPX_App;
        private GNPX_Engin	    pGNPX_Eng;
		private Research_trial  RTrial;

		public GNPX_AnalyzerMan AnMan => pGNPX_Eng.AnMan;

        private UPuzzle		  ePZL{ get=>pGNPX_Eng.ePZL; set=>pGNPX_Eng.ePZL=value; }

        public int            ID{ get=>ePZL.ID; }

      
		
		//===== Transform parameter =====
        private UPuzzle PZLorg; //original
        private UPuzzle PZLbas;
        public int[]    TrPara;
        private int[,]  RCX;
        public  int[]   ID_code;
      //----------------------------



        public GPuzzleTrans( GNPX_App_Man  pGNPX_App ){
            this.pGNPX_App = pGNPX_App;
			this.pGNPX_Eng = pGNPX_App.GNPX_Eng;
			this.RTrial    = new( AnMan );
        }




        public void Initialize( bool StartF=true ){          
            _Set_Solution_TrialAndError( ePZL );	 // TandE Solver

            TrPara=new int[18];       
            RCX=new int[4,12];
            for(int k=0; k<9; k++ ) RCX[0,k]=RCX[1,k]=k; 
            for(int k=0; k<3; k++ ) RCX[0,k+9]=RCX[1,k+9]=k*3;
            
            if(StartF) PZLorg = ePZL.Copy( );
            PZLbas = ePZL.Copy( );
			return;

						// ---------------------------------------------------------------------
						// Solve with "TrialAndError" and return the result(int array). Extremely fast! (msec order)
						int[] _Set_Solution_TrialAndError( UPuzzle aPZL ){
							List<int>  intBoard = aPZL.BOARD.ConvertAll(P=>Abs(P.No));
							RTrial = new Research_trial( AnMan );
							bool ret = RTrial.TrialAndErrorApp( intBoard, filePutB:true, upperLimit:2 );

							aPZL.AnsNum = ret? RTrial.RT_Get_Solution_iArray: null;
							aPZL.SolCode = 0;	
							return aPZL.AnsNum;
						}
        }

        public void btn_TransEst(){
            if(ePZL.AnsNum==null) return;
            Initialize(StartF:false);
        }
        public void btn_TransRes(){
            if(ePZL.AnsNum==null) return;
            PZLbas = PZLorg.Copy(0,0);
            UPuzzle tPZL = PZLorg.Copy(0,0);
            pGPGC.GNPX_Puzzle_List_Replace( ID, tPZL );
            ePZL = tPZL;
           
            TrPara=new int[18];           
            RCX=new int[4,12];
            for(int k=0; k<9; k++ ) RCX[0,k]=RCX[1,k]=k; 
            for(int k=0; k<3; k++ ) RCX[0,k+9]=RCX[1,k+9]=k*3;
        }
            
        public UPuzzle G_TransProbG( string ctrl, bool DspSolB ){
            if(ePZL.AnsNum==null) Initialize();

            int ixM=0, ixR=TrPara[8], ixC=1-ixR, nx, m, n;
            switch(ctrl){   
                case "NumChange": break;
                case "Checked": break;
                case "random":  break;

                case "btn_PatCVRg":
                    ixM=TrPara[0]=(++TrPara[0])%6;
                    for(int k=0; k<3; k++ )  RCX[ixR,k+9]=prmX[ixM,k]*3;
                    break;
                case "btn_PatCVR123g":
                    nx=RCX[ixR,9]; ixM=TrPara[1]=(++TrPara[1])%6;
                    for(int k=0; k<3; k++ )  RCX[ixR,k+nx]=prmX[ixM,k]+nx;
                    break;
                case "btn_PatCVR456g":
                    nx=RCX[ixR,10]; ixM=TrPara[2]=(++TrPara[2])%6;
                    for(int k=0; k<3; k++ )  RCX[ixR,k+nx]=prmX[ixM,k]+nx;
                    break;
                case "btn_PatCVR789g":
                    nx=RCX[ixR,11]; ixM=TrPara[3]=(++TrPara[3])%6;
                    for(int k=0; k<3; k++ )  RCX[ixR,k+nx]=prmX[ixM,k]+nx;
                    break;

                case "btn_PatCVCg":
                    ixM=TrPara[4]=(++TrPara[4])%6;
                    for(int k=0; k<3; k++ )  RCX[ixC,k+9]=prmX[ixM,k]*3;
                    break;
                case "btn_PatCVC123g":
                    nx=RCX[ixC,9]; ixM=TrPara[5]=(++TrPara[5])%6;                    
                    for(int k=0; k<3; k++ )  RCX[ixC,k+nx]=prmX[ixM,k]+nx;
                    break;
                case "btn_PatCVC456g":
                    nx=RCX[ixC,10]; ixM=TrPara[6]=(++TrPara[6])%6;
                    for(int k=0; k<3; k++ )  RCX[ixC,k+nx]=prmX[ixM,k]+nx;
                    break;
                case "btn_PatCVC789g":
                    nx=RCX[ixC,11]; ixM=TrPara[7]=(++TrPara[7])%6;
                    for(int k=0; k<3; k++ )  RCX[ixC,k+nx]=prmX[ixM,k]+nx;
                    break;
/*   
                case "btn_TransRtg": //clockwise
                    ixR=TrPara[8]=(++TrPara[8])%2;
                    ixC=1-ixR;
                    subTransLR(ixC);
                    if(ixR==0) subTransLR(ixC);
                    break;
*/
                case "btn_PatCVRCg":
                case "btn_PatCVRCg2":
                    ixR=TrPara[8]=(++TrPara[8])%2;
                    ixC=1-ixR;
                    if(ctrl=="btn_TransRtg") goto case "btn_TransLRg";//if clockwise,goto  horizontal flip
                    break;
                
                case "btn_TransLRg": //horizontal flip
                    subTransLR(ixC);
                    break; 
               
                case "btn_TransUDg": //vertical flip
                    subTransUD(ixR);
                    break;                        

                //Symmetric transformation
                case "btn_PatCVR123987g": //row 123
                    nx=RCX[ixR,9]; m=TrPara[1]; ixM=TrPara[1]=(m+1)%6;
                    for(int k=0; k<3; k++ )  RCX[ixR,k+nx]=prmX[ixM,k]+nx;
                    nx=RCX[ixR,11]; n=TrPara[3]; ixM=TrPara[3]=trsX[m,n];
                    for(int k=0; k<3; k++ )  RCX[ixR,k+nx]=prmX[ixM,k]+nx;
                    break;
                case "btn_PatCVC123987g": //Column 123
                    nx=RCX[ixC,9]; m=TrPara[5]; ixM=TrPara[5]=(m+1)%6;
                    for(int k=0; k<3; k++ )  RCX[ixC,k+nx]=prmX[ixM,k]+nx;
                    nx=RCX[ixC,11]; n=TrPara[7]; ixM=TrPara[7]=trsX[m,n];
                    for(int k=0; k<3; k++ )  RCX[ixC,k+nx]=prmX[ixM,k]+nx;
                    break;                 
                case "btn_PatCVR46g": //row 46
                    nx=RCX[ixR,10]; ixM=TrPara[2]=(TrPara[2]+3)%6;
                    for(int k=0; k<3; k++ )  RCX[ixR,k+nx]=prmX[ixM,k]+nx;
                    break;
                case "btn_PatCVC46g": //Column 46
                    nx=RCX[ixC,10]; ixM=TrPara[6]=(TrPara[6]+3)%6;
                    for(int k=0; k<3; k++ )  RCX[ixC,k+nx]=prmX[ixM,k]+nx;
                    break;     
            }

            for(int j=0; j<2; j++ ){
                for(int k=0; k<9; k++ ){
                    nx=RCX[j,k/3+9];
                    RCX[j+2,k] = RCX[j,nx+k%3];
                }
            }

            List<UCell> UCL=new List<UCell>();
            int[] AnsN2=new int[81];
            int   r, c, w;
            
            for(int rc=0; rc<81; rc++ ){
                r=RCX[ixR+2,rc/9]; c=RCX[ixC+2,rc%9];
                if(ixR==1){ w=r; r=c; c=w; }
                int rc2=r*9+c;
                UCell P=PZLbas.BOARD[rc2];
                UCL.Add( new UCell(rc,P.No,P.FreeB) );
                AnsN2[rc]=PZLbas.AnsNum[rc2];
            }
            UPuzzle aPZL=ePZL.Copy( );
            aPZL.BOARD=UCL; aPZL.AnsNum=AnsN2;
            if(ID>=0) pGPGC.GNPX_Puzzle_List_Replace( ID, aPZL );
            else{
                aPZL.ID = ePZL.ID = 0;
                pGPGC.GNPX_Puzzle_List_Add( aPZL );
            }
            if(!DspSolB) aPZL.BOARD.ForEach(P=>{P.No=Max(P.No,0);});
            pGPGC.current_Puzzle_No=ID;

            SetIDCode(TrPara,AnsN2);

                /*
                        string st="RCX:";
                        for(int j=0; j<4; j++ ){
                            for(int k=0; k<12; k++ ) st+=" "+RCX[j,k];
                            st+=" / ";
                        }
                        for(int k=0; k<9; k++ ) st+=" "+TrPara[k];
                        st+="//";
                        int[] BPw=new int[3];
                        for(int k=9; k<12; k++ ){BPw[k-9]=TrPara[k]; st+=" "+TrPara[k]; }
                        WriteLine(st);                 
                        
                        Bit81 BP=new Bit81(BPw);
                        WriteLine(BP);
                */
            return aPZL;
        }

        private void subTransUD( int ixR ){ 
            int ixM=TrPara[0]=(TrPara[0]+3)%6;
            for(int k=0; k<3; k++) RCX[ixR,k+9]=prmX[ixM,k]*3;

            int m=TrPara[1], n=TrPara[3];
            int nx=RCX[ixR,9]; ixM=TrPara[1]=(n+3)%6;
            for(int k=0; k<3; k++) RCX[ixR,k+nx]=prmX[ixM,k]+nx;

            nx=RCX[ixR,11]; ixM=TrPara[3]=(m+3)%6;
            for(int k=0; k<3; k++) RCX[ixR,k+nx]=prmX[ixM,k]+nx;

            nx=RCX[ixR,10]; ixM=TrPara[2]=(TrPara[2]+3)%6;
            for(int k=0; k<3; k++) RCX[ixR,k+nx]=prmX[ixM,k]+nx;
            return;
        }
        private void subTransLR( int ixC ){
            int ixM=TrPara[4]=(TrPara[4]+3)%6;
            for(int k=0; k<3; k++) RCX[ixC,k+9]=prmX[ixM,k]*3;

            int m=TrPara[5], n=TrPara[7];
            int nx=RCX[ixC,9]; ixM=TrPara[5]=(n+3)%6;
            for(int k=0; k<3; k++) RCX[ixC,k+nx]=prmX[ixM,k]+nx;

            nx=RCX[ixC,11]; ixM=TrPara[7]=(m+3)%6;
            for(int k=0; k<3; k++) RCX[ixC,k+nx]=prmX[ixM,k]+nx;

            nx=RCX[ixC,10]; ixM=TrPara[6]=(TrPara[6]+3)%6;
            for(int k=0; k<3; k++) RCX[ixC,k+nx]=prmX[ixM,k]+nx;
            return;
        }

        public void SetRCX( int mx, int[] TPw ){         
            int rx=TPw[8], cx=1-rx, nx, kx;
            switch(mx){ 
                case 0:                kx=TPw[0]; for(int k=0;k<3;k++) RCX[rx,k+9] =prmX[kx,k]*3;  break;   
                case 1: nx=RCX[rx,9];  kx=TPw[1]; for(int k=0;k<3;k++) RCX[rx,k+nx]=prmX[kx,k]+nx; break;
                case 2: nx=RCX[rx,10]; kx=TPw[2]; for(int k=0;k<3;k++) RCX[rx,k+nx]=prmX[kx,k]+nx; break;
                case 3: nx=RCX[rx,11]; kx=TPw[3]; for(int k=0;k<3;k++) RCX[rx,k+nx]=prmX[kx,k]+nx; break;
                case 4:                kx=TPw[4]; for(int k=0;k<3;k++) RCX[cx,k+9] =prmX[kx,k]*3;  break;
                case 5: nx=RCX[cx,9];  kx=TPw[5]; for(int k=0;k<3;k++) RCX[cx,k+nx]=prmX[kx,k]+nx; break;
                case 6: nx=RCX[cx,10]; kx=TPw[6]; for(int k=0;k<3;k++) RCX[cx,k+nx]=prmX[kx,k]+nx; break;
                case 7: nx=RCX[cx,11]; kx=TPw[7]; for(int k=0;k<3;k++) RCX[cx,k+nx]=prmX[kx,k]+nx; break;
                case 8: break;
            }
        }

        private void _G_TransIX( int[] TrPara, bool TransB=false, bool DspSolB=false ){
            int rx=TrPara[8], cx=1-rx;
            for(int j=0; j<2; j++ ){
                for(int k=0; k<9; k++ ){
                    int n=RCX[j,k/3+9];
                    RCX[j+2,k] = RCX[j,n+k%3];
                }
            }

            List<UCell> UCL=null;
            if(TransB) UCL=new List<UCell>();
            int [] AnsN2=new int[81];
            int   r, c, w;           
            for(int rc=0; rc<81; rc++ ){
                r=RCX[rx+2,rc/9]; c=RCX[cx+2,rc%9];
                if(rx==1){ w=r; r=c; c=w; }
                int rc2=r*9+c;
                AnsN2[rc]=PZLbas.AnsNum[rc2];
                if(TransB){
                    UCell P=PZLbas.BOARD[rc2];
                    UCL.Add( new UCell(rc,P.No,P.FreeB) );
                }
            }

            if(TransB){
                UPuzzle tPZL=ePZL.Copy( );
                tPZL.BOARD=UCL; tPZL.AnsNum=AnsN2;
                pGPGC.GNPX_Puzzle_List_Replace( ID, tPZL );
                if(!DspSolB) tPZL.BOARD.ForEach(P=>{P.No=Max(P.No,0);});
                pGPGC.current_Puzzle_No=ID;
            }

            SetIDCode(TrPara,AnsN2);
        }
        public void SetIDCode( int[] TP, int[] AnsNum ){
            TP[9]=TP[10]=TP[11]=0;
            for(int rc=0; rc<81; rc++ ) if(AnsNum[rc]>0) TP[9+rc/27] |= (1<<(26-rc%27));    

            int Q=0;
            for(int k=0; k<9; k++ ) Q = Q*10 + Abs(AnsNum[(k/3*9)+(k%3)]);
            TP[16]=Q;
        }

        public string G_Nomalize( bool DspSolB, bool NrmlNum ){
            int[]  TPw=new int[18];
            List<int[]> TPLst=new List<int[]>();
            if(ePZL.AnsNum==null) Initialize(); //Solve

          #region Standardization(Pattern)
            RCX=new int[4,12];
            for(int k=0; k<9; k++ ) RCX[0,k]=RCX[1,k]=k; 
            for(int k=0; k<3; k++ ) RCX[0,k+9]=RCX[1,k+9]=k*3;

            int minV=int.MaxValue;
        //===== step1 =====
            for(int tx=0; tx<2; tx++ ){
                TPw[8]=tx;
                for(int rx0=0; rx0<6; rx0++ ){
                    TPw[0]=rx0; SetRCX(0,TPw);
                    for(int rx1=0; rx1<6; rx1++ ){
                        TPw[1]=rx1; SetRCX(1,TPw);

                        for(int cx4=0; cx4<6; cx4++ ){
                            TPw[4]=cx4; SetRCX(4,TPw);
                            for(int cx5=0; cx5<6; cx5++ ){
                                TPw[5]=cx5; SetRCX(5,TPw);
                                _G_TransIX(TPw);

                                if( TPw[9]>minV ) continue;
                                minV=TPw[9];
                                int[]  TPtmp=new int[18];
                                TPw.CopyTo(TPtmp,0);
                                TPLst.Add(TPtmp);
                            }
                        }
                    }  
                }
            }          
            TPLst.Sort((A,B)=>(A[9]-B[9]));

        //===== step2 =====
            minV=TPLst[0][9];
            TPLst = TPLst.FindAll(P=> P[9]==minV).ToList();
            int TPLstCount=TPLst.Count;
            for(int hx=0; hx<TPLstCount; hx++ ){
                TPw[8]=TPLst[hx][8]; SetRCX(8,TPw);
                for(int mx=0; mx<18; mx++ ){
                    TPw[mx]=TPLst[hx][mx];
                    if(mx<9) SetRCX(mx,TPw);
                }
                for(int cx6=0; cx6<6; cx6++ ){
                    TPw[6]=cx6; SetRCX(6,TPw);
                    for(int cx7=0; cx7<6; cx7++ ){
                        TPw[7]=cx7; SetRCX(7,TPw);
                        _G_TransIX(TPw);

                        if(TPw[9]>minV) continue;
                        minV=TPw[9];
                        int[]  TPtmp=new int[18];
                        TPw.CopyTo(TPtmp,0);
                        TPLst.Add(TPtmp);
                    }  
                }
            }          
            TPLst.Sort((A,B)=>(A[9]-B[9]));

       //===== step3 =====
            minV=TPLst[0][9];
            TPLst = TPLst.FindAll(P=> P[9]==minV).ToList();
            minV=TPLst[0][10];
            TPLstCount=TPLst.Count;
            for(int hx=0; hx<TPLstCount; hx++ ){
                TPw[8]=TPLst[hx][8]; SetRCX(8,TPw);
                for(int mx=0; mx<18; mx++ ){
                    TPw[mx]=TPLst[hx][mx];
                    if(mx<9) SetRCX(mx,TPw);
                }
                for(int rx2=0; rx2<6; rx2++ ){
                    TPw[2]=rx2; SetRCX(2,TPw);
                    for(int rx3=0; rx3<6; rx3++ ){
                        TPw[3]=rx3; SetRCX(3,TPw);
                        _G_TransIX(TPw);

                        if(TPw[10]>minV) continue;
                        minV=TPw[10];
                        int[]  TPtmp=new int[18];
                        TPw.CopyTo(TPtmp,0);
                        TPLst.Add(TPtmp);
                    }  
                }
            }        
            TPLst.Sort((A,B)=>{
                if(A[10]!=B[10]) return (A[10]-B[10]);
                return (A[11]-B[11]);
            } );
            minV=TPLst[0][10];
            int minV1=TPLst[0][11];
            TPLst = TPLst.FindAll(P=> (P[10]==minV && P[11]==minV1)).ToList();

            string[] stLst=new string[TPLst.Count];
            for(int k=0; k<TPLst.Count; k++ ){
                TPLst[k].CopyTo(TrPara,0);
                SetRCX(8,TPw); 
                for(int mx=0; mx<9; mx++ ) SetRCX(mx,TrPara);
                _G_TransIX(TrPara,TransB:true,DspSolB:DspSolB);		// ++++++++++++++++

                string st=pGNPX_App.App_Ctrl.Get_SDKNumPattern(TrPara,ePZL.AnsNum);//Latin square	//**************
                st+=TransToString(TrPara);// ---------------
                stLst[k]=st;
                TrPara[17]=k;
                for(int n=0; n<TrPara.Count(); n++ ) TPLst[k][n]=TrPara[n];
                    /*
                            string st="RCX:";
                            for(int j=0; j<4; j++ ){
                                for(int m=0;m<12; m++ ) st+=" "+RCX[j,m];
                                st+=" / ";
                            }                      
                        
                            for(int m=0; m<9; m++ ) st+=" "+TrPara[m];
                            st+="//";
                            int[] IDC=new int[4];
                            for(int m=9; m<13; m++ ){ IDC[m-9]=TrPara[m]; st+=" "+TrPara[k]; }
                            WriteLine(st); 
                         // Bit81 BP=new Bit81(IDC);
                         // WriteLine(BP); 
                */
            }

            TPLst.Sort((A,B)=>{
                for(int k=9; k<A.Count(); k++ ) if(A[k]!=B[k]) return (A[k]-B[k]);
                return 0;
            } );

            TrPara=TPLst[0];	
            SetRCX(8,TrPara);
            for(int mx=0; mx<8; mx++ ) SetRCX(mx,TrPara);
          #endregion

            //Standardization(Latin square)
            _G_TransIX(TrPara,TransB:true,DspSolB:DspSolB);
            
            if(NrmlNum){
                int[] chgNum=new int[10];
                int NN=TrPara[15];
                for(int k=0; k<=9; k++ ){ chgNum[9-k]=NN%10; NN/=10; }
                for(int rc=0; rc<81; rc++ ){
                    int No=ePZL.AnsNum[rc];
                    UCell P=ePZL.BOARD[rc];
                    if(P.No>0) ePZL.AnsNum[rc]=P.No=chgNum[No];
                    else       ePZL.AnsNum[rc]=P.No=-chgNum[-No];
                }
            }
                /*
                            string st="◆";
                            for(int k=0; k<18; k++ ) st+=" "+TrPara[k];
                            WriteLine(st); 
                */
            return stLst[TrPara[17]];
        }


        public string TransToString( int[] TrPara ){
            string st=(TrPara[8]==0)? "": " Transposition\r";
            int idS=0;
            st+=" #"+(idS++)+" Change digits \r    ("+TrPara[16].ToString()+") -> (123456789)\r";
            if(TrPara[8]>0){ st+=$" #{(idS++)} Transpose row/column\r"; }
            for(int k=0; k<8; k++ ){
                int n=TrPara[k], m=0;
                if(n>0){
                    st+=$" #{(idS++)} ";
                    switch(k){
                        case 0: st+="Row Block";    break;
                        case 1: st+="Row";          break;
                        case 2: st+="Row"; m=6;     break;
                        case 3: st+="Row"; m=12;    break;
                        case 4: st+="Column Block"; break;
                        case 5: st+="Column";       break;
                        case 6: st+="Column"; m=6;  break;
                        case 7: st+="Column"; m=12; break;
                    }
                    st+=prmXst[n+m]+"\r";
                }
            }

            if(st!="")  st="\r\r======================\r"+st;
            st+="\r To restore the original figure,\r convert in reverse order.";
            return st;
        }

/*
===== Standadization Code===== sample
Pattern Code:
 2926772 21309292 51561358

Latin Square Code
 191888889 191888889 0

===== Transformation Parameter=====
Pattern: 1230 0014 1
Number:526793481 -> 123456789

======================
 Transposition
 #0 Change digits 
    (3080) -> (123456789)
 #1 Transpose row/column
 #2 Row Block(123)->(312)
 #3 Row(123)->(231)
 #4 Row4-6 exchange
 #5 Column(456)->(645)
 #6 Column8-9 exchange

 To restore the original figure,
 convert in reverse order.
 */
    }
}   