using System.Collections.Generic;
using ControllerDI.Services;
using Microsoft.AspNetCore.Mvc;
using Popbill;
using Popbill.Cashbill;

namespace CashbillExample.Controllers
{
    public class CashbillController : Controller
    {
        private readonly CashbillService _cashbillService;

        //링크허브에서 발급받은 고객사 고객사 인증정보로 링크아이디(LinkID)와 비밀키(SecretKey) 값을 변경하시기 바랍니다.
        private string linkID = "TESTER";
        private string secretKey = "SwWxqU+0TErBXy/9TVjIPEnI0VTUMMSQZtJf3Ed8q3I=";

        public CashbillController(CashbillInstance CBinstance)
        {
            //현금영수증 서비스 객체 생성
            _cashbillService = CBinstance.cashbillService;

            //연동환경 설정값, 개발용(true), 상업용(false)
            _cashbillService.IsTest = true;
        }

        //팝빌 연동회원 사업자번호 (하이픈 '-' 없이 10자리)
        string corpNum = "1234567890";

        //팝빌 연동회원 아이디
        string userID = "testkorea";

        /*
         * 현금영수증 Index page (Cashbill/Index.cshtml)
         */
        public IActionResult Index()
        {
            return View();
        }

        #region 현금영수증 / 취소현금영수증 발행 

        /*
         * 현금영수증 관리번호 중복여부를 확인합니다.
         * - 관리번호는 1~24자리로 숫자, 영문 '-', '_' 조합으로 구성할 수 있습니다.
         */
        public IActionResult CheckMgtKeyInUse()
        {
            try
            {
                // 현금영수증 문서관리번호
                string mgtKey = "20181030";

                bool result = _cashbillService.CheckMgtKeyInUse(corpNum, mgtKey);

                return View("result", result ? "사용중" : "미사용중");
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 1건의 현금영수증을 [즉시발행] 합니다.
         * - 발행일 기준 오후 5시 이전에 발행된 현금영수증은 다음날 오후 2시에 국세청 전송결과를 확인할 수 있습니다.
         * - 현금영수증 국세청 전송 정책에 대한 정보는 "[현금영수증 API 연동매뉴얼] > 1.4. 국세청 전송정책"을 참조하시기 바랍니다.
         * - 취소현금영수증 작성방법 안내 - http://blog.linkhub.co.kr/702
         */
        public IActionResult RegistIssue()
        {
            // 현금영수증 정보 객체 
            Cashbill cashbill = new Cashbill();

            // [필수] 문서관리번호, 사업자별로 중복되지 않도록 관리번호 할당
            // 1~24자리 영문,숫자,'-','_' 조합 구성
            cashbill.mgtKey = "20181122-001";

            // [취소거래시 필수] 원본 현금영수증 국세청승인번호
            cashbill.orgConfirmNum = "";

            // [취소거래시 필수] 원본 현금영수증 거래일자
            cashbill.orgTradeDate = "";

            // [필수] 문서형태, { 승인거래, 취소거래 } 중 기재
            cashbill.tradeType = "승인거래";

            // [필수] 거래구분, { 소득공제용, 지출증빙용 } 중 기재
            cashbill.tradeUsage = "소득공제용";

            // 거래유형, { 일반, 도서공연, 대중교통 } 중 기재
            cashbill.tradeOpt = "도서공연";

            // [필수] 과세형태, { 과세, 비과세 } 중 기재
            cashbill.taxationType = "과세";

            // [필수] 거래금액 ( 공급가액 + 세액 + 봉사료 ) 
            cashbill.totalAmount = "11000";

            // [필수] 공급가액
            cashbill.supplyCost = "10000";

            // [필수] 부가세 
            cashbill.tax = "1000";

            // [필수] 봉사료
            cashbill.serviceFee = "0";

            // [필수] 가맹점 사업자번호
            cashbill.franchiseCorpNum = corpNum;

            // 가맹점 상호
            cashbill.franchiseCorpName = "가맹점 상호";

            // 가맹점 대표자 성명
            cashbill.franchiseCEOName = "가맹점 대표자";

            // 가맹점 주소
            cashbill.franchiseAddr = "가맹점 주소";

            // 가맹점 전화번호
            cashbill.franchiseTEL = "070-1234-1234";

            // [필수] 식별번호
            // 거래구분(tradeUsage) - '소득공제용' 인 경우 주민등록/휴대폰/카드번호 기재 가능
            // 거래구분(tradeUsage) - '지출증빙용' 인 경우 사업자번호/주민등록/휴대폰/카드번호 기재 가능 
            cashbill.identityNum = "0101112222";

            // 주문자명
            cashbill.customerName = "주문자명";

            // 주문상품명
            cashbill.itemName = "주문상품명";

            // 주문번호
            cashbill.orderNumber = "주문번호";

            // 주문자 이메일
            cashbill.email = "test@test.com";

            // 주문자 휴대폰
            cashbill.hp = "010-111-222";

            // 주문자 팩스번호
            cashbill.fax = "02-6442-9700";

            // 발행시 알림문자 전송여부
            cashbill.smssendYN = false;

            // 현금영수증 발행 메모
            string memo = "현금영수증 즉시발행 메모";

            try
            {
                var response = _cashbillService.RegistIssue(corpNum, cashbill, memo, userID);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 1건의 현금영수증을 [임시저장] 합니다.
         * - [임시저장] 상태의 현금영수증은 발행(Issue API)을 호출해야만 국세청에 전송됩니다.
         * - 발행일 기준 오후 5시 이전에 발행된 현금영수증은 다음날 오후 2시에 국세청 전송결과를 확인할 수 있습니다.
         * - 현금영수증 국세청 전송 정책에 대한 정보는 "[현금영수증 API 연동매뉴얼] > 1.4. 국세청 전송정책"을 참조하시기 바랍니다.
         * - 취소현금영수증 작성방법 안내 - http://blog.linkhub.co.kr/702
         */
        public IActionResult Register()
        {
            // 현금영수증 정보 객체 
            Cashbill cashbill = new Cashbill();

            // [필수] 문서관리번호, 사업자별로 중복되지 않도록 관리번호 할당
            // 1~24자리 영문,숫자,'-','_' 조합 구성
            cashbill.mgtKey = "20181122-002";

            // [취소거래시 필수] 원본 현금영수증 국세청승인번호
            cashbill.orgConfirmNum = "";

            // [취소거래시 필수] 원본 현금영수증 거래일자
            cashbill.orgTradeDate = "";

            // [필수] 문서형태, { 승인거래, 취소거래 } 중 기재
            cashbill.tradeType = "승인거래";

            // [필수] 거래구분, { 소득공제용, 지출증빙용 } 중 기재
            cashbill.tradeUsage = "소득공제용";

            // 거래유형, { 일반, 도서공연, 대중교통 } 중 기재
            cashbill.tradeOpt = "도서공연";

            // [필수] 과세형태, { 과세, 비과세 } 중 기재
            cashbill.taxationType = "과세";

            // [필수] 거래금액 ( 공급가액 + 세액 + 봉사료 ) 
            cashbill.totalAmount = "11000";

            // [필수] 공급가액
            cashbill.supplyCost = "10000";

            // [필수] 부가세 
            cashbill.tax = "1000";

            // [필수] 봉사료
            cashbill.serviceFee = "0";

            // [필수] 가맹점 사업자번호
            cashbill.franchiseCorpNum = corpNum;

            // 가맹점 상호
            cashbill.franchiseCorpName = "가맹점 상호";

            // 가맹점 대표자 성명
            cashbill.franchiseCEOName = "가맹점 대표자";

            // 가맹점 주소
            cashbill.franchiseAddr = "가맹점 주소";

            // 가맹점 전화번호
            cashbill.franchiseTEL = "070-1234-1234";

            // [필수] 식별번호
            // 거래구분(tradeUsage) - '소득공제용' 인 경우 주민등록/휴대폰/카드번호 기재 가능
            // 거래구분(tradeUsage) - '지출증빙용' 인 경우 사업자번호/주민등록/휴대폰/카드번호 기재 가능 
            cashbill.identityNum = "0101112222";

            // 주문자명
            cashbill.customerName = "주문자명";

            // 주문상품명
            cashbill.itemName = "주문상품명";

            // 주문번호
            cashbill.orderNumber = "주문번호";

            // 주문자 이메일
            cashbill.email = "test@test.com";

            // 주문자 휴대폰
            cashbill.hp = "010-111-222";

            // 주문자 팩스번호
            cashbill.fax = "02-6442-9700";

            // 발행시 알림문자 전송여부
            cashbill.smssendYN = false;

            try
            {
                var response = _cashbillService.Register(corpNum, cashbill, userID);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 1건의 현금영수증을 [수정] 합니다.
         * - [임시저장] 상태의 현금영수증만 수정할 수 있습니다.
         * - 국세청에 신고된 현금영수증은 수정할 수 없으며, 취소 현금영수증을 발행하여 취소처리 할 수 있습니다.
         * - 취소현금영수증 작성방법 안내 - http://blog.linkhub.co.kr/702
         */
        public IActionResult Update()
        {
            // 수정할 현금영수증 문서관리번호, 사업자별로 중복되지 않도록 관리번호 할당
            // 1~24자리 영문,숫자,'-','_' 조합 구성
            string mgtKey = "20181122-002";

            
            // 현금영수증 정보 객체 
            Cashbill cashbill = new Cashbill();

            // [취소거래시 필수] 원본 현금영수증 국세청승인번호
            cashbill.orgConfirmNum = "";

            // [취소거래시 필수] 원본 현금영수증 거래일자
            cashbill.orgTradeDate = "";

            // [필수] 문서형태, { 승인거래, 취소거래 } 중 기재
            cashbill.tradeType = "승인거래";

            // [필수] 거래구분, { 소득공제용, 지출증빙용 } 중 기재
            cashbill.tradeUsage = "소득공제용";

            // 거래유형, { 일반, 도서공연, 대중교통 } 중 기재
            cashbill.tradeOpt = "도서공연";

            // [필수] 과세형태, { 과세, 비과세 } 중 기재
            cashbill.taxationType = "과세";

            // [필수] 거래금액 ( 공급가액 + 세액 + 봉사료 ) 
            cashbill.totalAmount = "11000";

            // [필수] 공급가액
            cashbill.supplyCost = "10000";

            // [필수] 부가세 
            cashbill.tax = "1000";

            // [필수] 봉사료
            cashbill.serviceFee = "0";

            // [필수] 가맹점 사업자번호
            cashbill.franchiseCorpNum = corpNum;

            // 가맹점 상호
            cashbill.franchiseCorpName = "가맹점 상호";

            // 가맹점 대표자 성명
            cashbill.franchiseCEOName = "가맹점 대표자";

            // 가맹점 주소
            cashbill.franchiseAddr = "가맹점 주소";

            // 가맹점 전화번호
            cashbill.franchiseTEL = "070-1234-1234";

            // [필수] 식별번호
            // 거래구분(tradeUsage) - '소득공제용' 인 경우 주민등록/휴대폰/카드번호 기재 가능
            // 거래구분(tradeUsage) - '지출증빙용' 인 경우 사업자번호/주민등록/휴대폰/카드번호 기재 가능 
            cashbill.identityNum = "0101112222";

            // 주문자명
            cashbill.customerName = "주문자명";

            // 주문상품명
            cashbill.itemName = "주문상품명";

            // 주문번호
            cashbill.orderNumber = "주문번호";

            // 주문자 이메일
            cashbill.email = "test@test.com";

            // 주문자 휴대폰
            cashbill.hp = "010-111-222";

            // 주문자 팩스번호
            cashbill.fax = "02-6442-9700";

            // 발행시 알림문자 전송여부
            cashbill.smssendYN = false;

            try
            {
                var response = _cashbillService.Update(corpNum, mgtKey, cashbill, userID);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 1건의 임시저장 현금영수증을 [발행] 합니다.
         * - 발행일 기준 오후 5시 이전에 발행된 현금영수증은 다음날 오후 2시에 국세청 전송결과를 확인할 수 있습니다.
         * - 현금영수증 국세청 전송 정책에 대한 정보는 "[현금영수증 API 연동매뉴얼] > 1.4. 국세청 전송정책"을 참조하시기 바랍니다.
         */
        public IActionResult Issue()
        {
            // 현금영수증 문서관리번호
            string mgtKey = "20181122-002";

            // 메모
            string memo = "발행 메모";

            try
            {
                var response = _cashbillService.Issue(corpNum, mgtKey, memo, userID);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * [발행완료] 상태의 현금영수증을 [발행취소] 합니다.
         * - 발행취소는 국세청 전송전에만 가능합니다.
         * - 발행취소된 형금영수증은 국세청에 전송되지 않습니다.
         */
        public IActionResult CancelIssue()
        {
            // 현금영수증 문서관리번호
            string mgtKey = "20181122-001";

            // 메모
            string memo = "발행취소 메모";

            try
            {
                var response = _cashbillService.CancelIssue(corpNum, mgtKey, memo, userID);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 1건의 현금영수증을 [삭제] 합니다.
         * - 현금영수증을 삭제하면 사용된 문서관리번호(mgtKey)를 재사용할 수 있습니다.
         * - 삭제가능한 문서 상태 : [임시저장], [발행취소]
         */
        public IActionResult Delete()
        {
            // 현금영수증 문서관리번호
            string mgtKey = "20181122-001";

            try
            {
                var response = _cashbillService.Delete(corpNum, mgtKey, userID);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 1건의 취소현금영수증을 [즉시발행] 합니다.
         * - 발행일 기준 오후 5시 이전에 발행된 현금영수증은 다음날 오후 2시에 국세청 전송결과를 확인할 수 있습니다.
         * - 현금영수증 국세청 전송 정책에 대한 정보는 "[현금영수증 API 연동매뉴얼] > 1.4. 국세청 전송정책"을 참조하시기 바랍니다.
         * - 취소현금영수증 작성방법 안내 - http://blog.linkhub.co.kr/702
         */
        public IActionResult RevokeRegistIssue()
        {
            // 현금영수증 문서관리번호
            string mgtKey = "20181124185045";

            // 원본 현금영수증 국세청승인번호
            string orgConfirmNum = "820116333";

            // 원본현금영수증 거래일자 (날짜형식yyyyMMdd)
            string orgTradeDate = "20170711";

            try
            {
                var response = _cashbillService.RevokeRegistIssue(corpNum, mgtKey, orgConfirmNum, orgTradeDate);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 1건의 (부분)취소현금영수증을 [즉시발행] 합니다.
         * - 발행일 기준 오후 5시 이전에 발행된 현금영수증은 다음날 오후 2시에 국세청 전송결과를 확인할 수 있습니다.
         * - 현금영수증 국세청 전송 정책에 대한 정보는 "[현금영수증 API 연동매뉴얼] > 1.4. 국세청 전송정책"을 참조하시기 바랍니다.
         * - 취소현금영수증 작성방법 안내 - http://blog.linkhub.co.kr/702
         */
        public IActionResult RevokeRegistIssue_part()
        {
            // 현금영수증 문서관리번호
            string mgtKey = "20181124185045";

            // 원본 현금영수증 국세청승인번호
            string orgConfirmNum = "820116333";

            // 원본현금영수증 거래일자 (날짜형식yyyyMMdd)
            string orgTradeDate = "20170711";

            // 발행 안내문자 전송여부           
            bool smssendYN = false;

            // 메모
            string memo = "부분 취소발행 메모";

            // 부분취소여부, true-부분취소, false-전체취소
            bool isPartCancel = true;

            // 취소사유, 1-거래취소, 2-오류발급취소, 3-기타
            int cancelType = 1;

            // [취소] 공급가액
            string supplyCost = "3000";

            // [취소] 부가세
            string tax = "300";

            // [취소] 봉사료
            string serviceFee = "";

            // [취소] 합계금액
            string totalAmount = "3300";

            try
            {
                var response = _cashbillService.RevokeRegistIssue(corpNum, mgtKey, orgConfirmNum, orgTradeDate,
                    smssendYN, memo, isPartCancel, cancelType, totalAmount, supplyCost, tax, serviceFee, userID);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 1건의 취소현금영수증을 [임시저장] 합니다.
         * - [임시저장] 상태의 현금영수증은 발행(Issue API)을 호출해야만 국세청에 전송됩니다.
         * - 발행일 기준 오후 5시 이전에 발행된 현금영수증은 다음날 오후 2시에 국세청 전송결과를 확인할 수 있습니다.
         * - 현금영수증 국세청 전송 정책에 대한 정보는 "[현금영수증 API 연동매뉴얼] > 1.4. 국세청 전송정책"을 참조하시기 바랍니다.
         * - 취소현금영수증 작성방법 안내 - http://blog.linkhub.co.kr/702
         */
        public IActionResult RevokeRegister()
        {
            // 현금영수증 문서관리번호
            string mgtKey = "R20181003-101";

            // 원본 현금영수증 국세청승인번호
            string orgConfirmNum = "662216870";

            // 원본현금영수증 거래일자 (날짜형식yyyyMMdd)
            string orgTradeDate = "20181121";

            try
            {
                var response = _cashbillService.RevokeRegister(corpNum, mgtKey, orgConfirmNum, orgTradeDate);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 1건의 (부분)취소현금영수증을 [임시저장] 합니다.
         * - [임시저장] 상태의 현금영수증은 발행(Issue API)을 호출해야만 국세청에 전송됩니다.
         * - 발행일 기준 오후 5시 이전에 발행된 현금영수증은 다음날 오후 2시에 국세청 전송결과를 확인할 수 있습니다.
         * - 현금영수증 국세청 전송 정책에 대한 정보는 "[현금영수증 API 연동매뉴얼] > 1.4. 국세청 전송정책"을 참조하시기 바랍니다.
         * - 취소현금영수증 작성방법 안내 - http://blog.linkhub.co.kr/702
         */
        public IActionResult RevokeRegister_part()
        {
            // 현금영수증 문서관리번호
            string mgtKey = "R20181003-100";

            // 원본 현금영수증 국세청승인번호
            string orgConfirmNum = "662216870";

            // 원본현금영수증 거래일자 (날짜형식yyyyMMdd)
            string orgTradeDate = "20181121";

            // 발행 안내문자 전송여부           
            bool smssendYN = false;

            // 메모
            string memo = "부분 취소발행 메모";

            // 부분취소여부, true-부분취소, false-전체취소
            bool isPartCancel = true;

            // 취소사유, 1-거래취소, 2-오류발급취소, 3-기타
            int cancelType = 1;

            // [취소] 공급가액
            string supplyCost = "3000";

            // [취소] 부가세
            string tax = "300";

            // [취소] 봉사료
            string serviceFee = "";

            // [취소] 합계금액
            string totalAmount = "3300";

            try
            {
                var response = _cashbillService.RevokeRegister(corpNum, mgtKey, orgConfirmNum, orgTradeDate,
                    smssendYN, isPartCancel, cancelType, totalAmount, supplyCost, tax, serviceFee, userID);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        #endregion

        #region 현금영수증 정보확인 

        /*
         * 1건의 현금영수증 상태/요약 정보를 확인합니다.
         * - 응답항목에 대한 자세한 정보는 "[현금영수증 API 연동매뉴얼] > 4.2. 현금영수증 상태정보 구성"을 참조하시기 바랍니다.
         */
        public IActionResult GetInfo()
        {
            // 현금영수증 문서관리번호
            string mgtKey = "20181003-100";

            try
            {
                var response = _cashbillService.GetInfo(corpNum, mgtKey, userID);
                return View("GetInfo", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 대량의 현금영수증 상태/요약 정보를 확인합니다. (최대 1000건)
         * - 응답항목에 대한 자세한 정보는 "[현금영수증 API 연동매뉴얼] > 4.2. 현금영수증 상태정보 구성"을 참조하시기 바랍니다.
         */
        public IActionResult GetInfos()
        {
            // 조회할 현금영수증 문서관리번호 배열, (최대 1000건)
            List<string> mgtKeyList = new List<string> {"20181003-100", "WLJ181108-00000002", "NLJ81029-80592698"};

            try
            {
                var response = _cashbillService.GetInfos(corpNum, mgtKeyList, userID);
                return View("GetInfos", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 현금영수증 1건의 상세정보를 조회합니다.
         * - 응답항목에 대한 자세한 사항은 "[현금영수증 API 연동매뉴얼] > 4.1. 현금영수증 구성" 을 참조하시기 바랍니다.
         */
        public IActionResult GetDetailInfo()
        {
            // 현금영수증 문서관리번호
            string mgtKey = "20181003-100";

            try
            {
                var response = _cashbillService.GetDetailInfo(corpNum, mgtKey, userID);
                return View("GetDetailInfo", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 검색조건을 사용하여 현금영수증 목록을 조회합니다.
         * - 응답항목에 대한 자세한 사항은 "[현금영수증 API 연동매뉴얼] > 4.2. 현금영수증 상태정보 구성" 을 참조하시기 바랍니다.
         */
        public IActionResult Search()
        {
            // 검색일자 유형, R-등록일자, T-거래일자, I-발행일자
            string DType = "T";

            // 시작일자, 날짜형식(yyyyMMdd)
            string SDate = "20181101";

            // 종료일자, 날짜형식(yyyyMMdd)
            string EDate = "20181124";

            // 상태코드 배열, 2,3번째 자리에 와일드카드(*) 사용가능
            // - 상태코드에 대한 자세한 사항은 "[현금영수증 API 연동매뉴얼] >
            //   5.1. 현금영수증 상태코드"를 참고하시기 바랍니다.
            string[] State = new string[3];
            State[0] = "1**";
            State[1] = "3**";
            State[2] = "4**";

            // 문서형태 배열, N-일반 현금영수증, C-취소 현금영수증
            string[] TradeType = new string[2];
            TradeType[0] = "N";
            TradeType[1] = "C";

            // 거래구분 배열, P-소득공제용, C-지출증빙용
            string[] TradeUsage = new string[2];
            TradeUsage[0] = "P";
            TradeUsage[1] = "C";

            // 거래유형 배열, N-일반, B-도서공연, T-대중교통
            string[] TradeOpt = new string[3];
            TradeOpt[0] = "N";
            TradeOpt[1] = "B";
            TradeOpt[2] = "T";

            // 과세형태 배열, T-과세, N-비과세 
            string[] TaxationType = new string[2];
            TaxationType[0] = "T";
            TaxationType[1] = "N";

            // 페이지 번호, 기본값 '1'
            int Page = 1;

            // 페이지당 검색개수, 기본값 '500', 최대 '1000' 
            int PerPage = 30;

            // 정렬방향, D-내림차순, A-오름차순
            string Order = "D";

            // 식별번호 조회, 미기재시 전체조회 
            string QString = "";

            try
            {
                var response = _cashbillService.Search(corpNum, DType, SDate, EDate, State, TradeType, TradeUsage,
                    TradeOpt, TaxationType, Page, PerPage, Order, QString, userID);
                return View("Search", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 현금영수증 상태 변경이력을 확인합니다.
         * - 상태 변경이력 확인(GetLogs API) 응답항목에 대한 자세한 정보는
         *   "[현금영수증 API 연동매뉴얼] > 3.4.4 상태 변경이력 확인" 을 참조하시기 바랍니다.
         */
        public IActionResult GetLogs()
        {
            // 현금영수증 문서관리번호
            string mgtKey = "20181003-100";

            try
            {
                var response = _cashbillService.GetLogs(corpNum, mgtKey, userID);
                return View("GetLogs", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 팝빌 현금영수증 문서함 팝업 URL을 반환합니다.
         * - 반환된 URL의 유지시간은 30초이며, 제한된 시간 이후에는 정상적으로 처리되지 않습니다.
         */
        public IActionResult GetURL()
        {
            // 현금영수증 문서관리번호
            string mgtKey = "20181124185045";

            // TBOX(임시문서함), PBOX(발행문서함), WRITE(현금영수증 작성)
            string TOGO = "WRITE";

            try
            {
                var result = _cashbillService.GetURL(corpNum, TOGO, userID);
                return View("Result", result);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        #endregion

        #region 현금영수증 보기/인쇄

        /*
         * 1건의 현금영수증 보기 팝업 URL을 반환합니다.
         * - 반환된 URL의 유지시간은 30초이며, 제한된 시간 이후에는 정상적으로 처리되지 않습니다.
         */
        public IActionResult GetPopUpURL()
        {
            // 현금영수증 문서관리번호
            string mgtKey = "20181124185045";

            try
            {
                var result = _cashbillService.GetPopUpURL(corpNum, mgtKey, userID);
                return View("Result", result);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 1건의 현금영수증 인쇄팝업 URL을 반환합니다.
         * - 반환된 URL의 유지시간은 30초이며, 제한된 시간 이후에는 정상적으로 처리되지 않습니다.
         */
        public IActionResult GetPrintURL()
        {
            // 현금영수증 문서관리번호
            string mgtKey = "20181124185045";
            try
            {
                var result = _cashbillService.GetPrintURL(corpNum, mgtKey, userID);
                return View("Result", result);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 대량의 현금영수증 인쇄팝업 URL을 반환합니다. (최대 100건)
         * - 반환된 URL의 유지시간은 30초이며, 제한된 시간 이후에는 정상적으로 처리되지 않습니다.
         */
        public IActionResult GetMassPrintURL()
        {
            // 조회할 현금영수증 문서관리번호 배열, (최대 1000건)
            List<string> MgtKeyList = new List<string> {"20181124-001", "20181124-002", "20181124-003"};

            try
            {
                var result = _cashbillService.GetMassPrintURL(corpNum, MgtKeyList, userID);
                return View("Result", result);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 공급받는자 메일링크 URL을 반환합니다.
         * - 메일링크 URL은 유효시간이 존재하지 않습니다.
         */
        public IActionResult GetMailURL()
        {
            // 현금영수증 문서관리번호
            string mgtKey = "20181124185045";
            try
            {
                var result = _cashbillService.GetMailURL(corpNum, mgtKey, userID);
                return View("Result", result);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        #endregion

        #region 부가기능

        /*
          * 팝빌에 로그인 상태로 접근할 수 있는 팝업 URL을 반환합니다.
          * - 반환된 URL의 유지시간은 30초이며, 제한된 시간 이후에는 정상적으로 처리되지 않습니다.
          */
        public IActionResult GetAccessURL()
        {
            try
            {
                var result = _cashbillService.GetAccessURL(corpNum, userID);
                return View("Result", result);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 현금영수증 발행 안내메일을 재전송합니다.
         */
        public IActionResult SendEmail()
        {
            // 현금영수증 문서관리번호
            string mgtKey = "20181124185045";

            // 수신자 이메일주소
            string receiveEmail = "test@test.com";

            try
            {
                var result = _cashbillService.SendEmail(corpNum, mgtKey, receiveEmail, userID);
                return View("Result", result);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 알림문자를 전송합니다. (단문/SMS - 한글 최대 45자)
         * - 알림문자 전송시 포인트가 차감됩니다. (전송실패시 환불처리)
         * - 전송내역 확인은 "팝빌 로그인" > [문자 팩스] > [전송내역] 탭에서 전송결과를 확인할 수 있습니다.
         */
        public IActionResult SendSMS()
        {
            // 발신자 번호
            string sender = "070-4304-2991";

            // 수신자 번호
            string receiver = "010-111-222";

            // 메시지 내용, 90byte 초과시 길이가 조정되어 전송됨
            string contents = "문자 메시지 내용은 90byte초과시 길이가 조정되어 전송됩니다.";

            try
            {
                var result = _cashbillService.SendSMS(corpNum, sender, receiver, contents, userID);
                return View("Result", result);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 현금영수증을 팩스전송합니다.
         * - 팩스 전송 요청시 포인트가 차감됩니다. (전송실패시 환불처리)
         * - 전송내역 확인은 "팝빌 로그인" > [문자 팩스] > [팩스] > [전송내역] 메뉴에서 전송결과를 확인할 수 있습니다.
         */
        public IActionResult SendFAX()
        {
            // 발신번호
            string sender = "070-4304-2991";

            // 수신번호
            string receiver = "010-111-222";

            try
            {
                var result = _cashbillService.SendFAX(corpNum, sender, receiver, userID);
                return View("Result", result);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 현금영수증 관련 메일전송 항목에 대한 전송여부를 목록을 반환합니다.
         */
        public IActionResult ListEmailConfig()
        {
            try
            {
                var result = _cashbillService.ListEmailConfig(corpNum, userID);
                return View("ListEmailConfig", result);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 현금영수증 관련 메일전송 항목에 대한 전송여부를 수정합니다.
         *
         * 메일전송유형
         * CSH_ISSUE : 고객에게 현금영수증이 발행 되었음을 알려주는 메일 입니다.
         * CSH_CANCEL : 고객에게 현금영수증이 발행취소 되었음을 알려주는 메일 입니다.
         */
        public IActionResult UpdateEmailConfig()
        {
            //메일전송유형
            string emailType = "CSH_ISSUE";

            //전송여부 (true-전송, false-미전송)
            bool sendYN = true;

            try
            {
                var response = _cashbillService.UpdateEmailConfig(corpNum, emailType, sendYN, userID);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        #endregion

        #region 포인트관리 

        /*
         * 연동회원 잔여포인트를 확인합니다.
         */
        public IActionResult GetBalance()
        {
            try
            {
                var result = _cashbillService.GetBalance(corpNum);
                return View("Result", result);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 팝빌 연동회원의 포인트충전 팝업 URL을 반환합니다.
         * - 반환된 URL의 유지시간은 30초이며, 제한된 시간 이후에는 정상적으로 처리되지 않습니다.
         */
        public IActionResult GetChargeURL()
        {
            try
            {
                var result = _cashbillService.GetChargeURL(corpNum, userID);
                return View("Result", result);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 파트너의 잔여포인트를 확인합니다.
         * - 과금방식이 연동과금인 경우 연동회원 잔여포인트(GetBalance API)를
         *   이용하시기 바랍니다.
         */
        public IActionResult GetPartnerBalance()
        {
            try
            {
                var result = _cashbillService.GetPartnerBalance(corpNum);
                return View("Result", result);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 파트너 포인트 충전 팝업 URL을 반환합니다.
         * - 반환된 URL의 유지시간은 30초이며, 제한된 시간 이후에는 정상적으로 처리되지 않습니다.
         */
        public IActionResult GetPartnerURL()
        {
            // CHRG 포인트충전 URL
            string TOGO = "CHRG";

            try
            {
                var result = _cashbillService.GetPartnerURL(corpNum, TOGO);
                return View("Result", result);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 현금영수증 발행단가를 확인합니다.
         */
        public IActionResult GetUnitCost()
        {
            try
            {
                var result = _cashbillService.GetUnitCost(corpNum);
                return View("Result", result);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 현금영수증 API 서비스 과금정보를 확인합니다.
         */
        public IActionResult GetChargeInfo()
        {
            try
            {
                var response = _cashbillService.GetChargeInfo(corpNum);
                return View("GetChargeInfo", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        #endregion

        #region 회원정보

        /*
         * 해당 사업자의 파트너 연동회원 가입여부를 확인합니다.
         */
        public IActionResult CheckIsMember()
        {
            try
            {
                //링크아이디
                string linkID = "TESTER";

                var response = _cashbillService.CheckIsMember(corpNum, linkID);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 팝빌 회원아이디 중복여부를 확인합니다.
         */
        public IActionResult CheckID()
        {
            //중복여부 확인할 팝빌 회원 아이디
            string checkID = "testkorea";

            try
            {
                var response = _cashbillService.CheckID(checkID);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 파트너의 연동회원으로 회원가입을 요청합니다.
         */
        public IActionResult JoinMember()
        {
            JoinForm joinInfo = new JoinForm
            {
                LinkID = "TESTER", // 링크아이디
                ID = "userid", // 아이디, 6자이상 50자 미만
                PWD = "12341234", // 비밀번호, 6자이상 20자 미만
                CorpNum = "0000000001", // 사업자번호 "-" 제외
                CEOName = "대표자 성명", // 대표자 성명 
                CorpName = "상호", // 상호
                Addr = "주소", // 주소
                BizType = "업태", // 업태
                BizClass = "종목", // 종목
                ContactName = "담당자명", // 담당자 성명 
                ContactEmail = "test@test.com", // 담당자 이메일주소         
                ContactTEL = "070-4304-2992", // 담당자 연락처   
                ContactHP = "010-111-222", // 담당자 휴대폰번호 
                ContactFAX = "02-111-222" // 담당자 팩스번호
            };

            try
            {
                var response = _cashbillService.JoinMember(joinInfo);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 연동회원의 회사정보를 확인합니다.
         */
        public IActionResult GetCorpInfo()
        {
            try
            {
                var response = _cashbillService.GetCorpInfo(corpNum, userID);
                return View("GetCorpInfo", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 연동회원의 회사정보를 수정합니다
         */
        public IActionResult UpdateCorpInfo()
        {
            CorpInfo corpInfo = new CorpInfo
            {
                ceoname = "대표자 성명 수정", // 대표자 성명
                corpName = "상호 수정", // 상호
                addr = "주소 수정", // 주소
                bizType = "업태 수정", // 업태 
                bizClass = "종목 수정" // 종목
            };

            try
            {
                var response = _cashbillService.UpdateCorpInfo(corpNum, corpInfo, userID);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 연동회원의 담당자를 신규로 등록합니다.
         */
        public IActionResult RegistContact()
        {
            Contact contactInfo = new Contact
            {
                id = "testkorea_20181108", // 담당자 아이디, 6자 이상 50자 미만
                pwd = "user_password", // 비밀번호, 6자 이상 20자 미만
                personName = "코어담당자", // 담당자명
                tel = "070-4304-2992", // 담당자연락처
                hp = "010-111-222", // 담당자 휴대폰번호
                fax = "02-111-222", // 담당자 팩스번호 
                email = "netcore@linkhub.co.kr", // 담당자 메일주소
                searchAllAllowYN = true, // 회사조회 권한여부, true(회사조회), false(개인조회)
                mgrYN = false // 관리자 권한여부 
            };

            try
            {
                var response = _cashbillService.RegistContact(corpNum, contactInfo, userID);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 연동회원의 담당자 목록을 확인합니다.
         */
        public IActionResult ListContact()
        {
            try
            {
                var response = _cashbillService.ListContact(corpNum, userID);
                return View("ListContact", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 연동회원의 담당자 정보를 수정합니다.
         */
        public IActionResult UpdateContact()
        {
            Contact contactInfo = new Contact
            {
                id = "testkorea", // 아이디
                personName = "담당자명", // 담당자명 
                tel = "070-4304-2992", // 연락처
                hp = "010-222-111", // 휴대폰번호
                fax = "02-222-1110", // 팩스번호
                email = "aspnetcore@popbill.co.kr", // 이메일주소
                searchAllAllowYN = true, // 회사조회 권한여부, true(회사조회), false(개인조회)
                mgrYN = false // 관리자 권한여부 
            };

            try
            {
                var response = _cashbillService.UpdateContact(corpNum, contactInfo, userID);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        #endregion
    }
}