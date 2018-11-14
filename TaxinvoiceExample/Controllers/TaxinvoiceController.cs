using System;
using System.Collections.Generic;
using ControllerDI.Services;
using Microsoft.AspNetCore.Mvc;
using Popbill;
using Popbill.Taxinvoice;

namespace TaxinvoiceExample.Controllers
{
    public class TaxinvoiceController : Controller
    {
        private readonly TaxinvoiceService _taxinvoiceService;

        //링크허브에서 발급받은 고객사 고객사 인증정보로 링크아이디(LinkID)와 비밀키(SecretKey) 값을 변경하시기 바랍니다.
        private string linkID = "TESTER";
        private string secretKey = "SwWxqU+0TErBXy/9TVjIPEnI0VTUMMSQZtJf3Ed8q3I=";

        public TaxinvoiceController(TaxinvoiceInstance TIinstance)
        {
            //세금계산서 서비스 객체 생성
            _taxinvoiceService = TIinstance.getInstance(linkID, secretKey);
            
            //연동환경 설정값, 개발용(true), 상업용(false)
            _taxinvoiceService.IsTest = true;
        }

        //팝빌 연동회원 사업자번호 (하이픈 '-' 없이 10자리)
        string corpNum = "1234567890";

        //팝빌 연동회원 아이디
        string userID = "testkorea";

        /*
         * 전자세금계산서 Index page (Taxinvoice/Index.cshtml)
         */
        public IActionResult Index()
        {
            return View();
        }

        #region 정방행/역발행/위수탁발행

        /*
         * 세금계산서 관리번호 중복여부를 확인합니다.
         * - 관리번호는 1~24자리로 숫자, 영문 '-', '_' 조합으로 구성할 수 있습니다.
         */
        public IActionResult CheckMgtKeyInUse()
        {
            try
            {
                // 세금계산서 문서관리번호
                string mgtKey = "20181030";

                // 세금계산서유형, SELL(매출), BUY(매입), TRUSTEE(위수탁)
                MgtKeyType mgtKeyType = MgtKeyType.SELL;

                bool result = _taxinvoiceService.CheckMgtKeyInUse(corpNum, mgtKeyType, mgtKey);

                return View("result", result ? "사용중" : "미사용중");
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 1건의 세금계산서를 [즉시발행]합니다.
         * - 세금계산서 항목별 정보는 "[전자세금계산서 API 연동매뉴얼] > 4.1. (세금)계산서구성"을 참조하시기 바랍니다.
         */
        public IActionResult RegistIssue()
        {
            // 세금계산서 정보 객체 
            Taxinvoice taxinvoice = new Taxinvoice();

            // [필수] 기재상 작성일자, 날짜형식(yyyyMMdd)
            taxinvoice.writeDate = "20181113";

            // [필수] 과금방향, {정과금, 역과금}중 선택
            // - 정과금(공급자과금), 역과금(공급받는자과금)
            // - 역과금은 역발행 세금계산서를 발행하는 경우만 가능
            taxinvoice.chargeDirection = "정과금";

            // [필수] 발행형태, {정발행, 역발행, 위수탁} 중 기재 
            taxinvoice.issueType = "정발행";

            // [필수] {영수, 청구} 중 기재
            taxinvoice.purposeType = "영수";

            // [필수] 발행시점, {직접발행, 승인시자동발행} 중 기재 
            // - {승인시자동발행}은 발행예정 프로세스에서만 이용가능
            taxinvoice.issueTiming = "직접발행";

            // [필수] 과세형태, {과세, 영세, 면세} 중 기재
            taxinvoice.taxType = "과세";

            
            /*****************************************************************
             *                         공급자 정보                           *
             *****************************************************************/

            // [필수] 공급자 사업자번호, '-' 제외 10자리
            taxinvoice.invoicerCorpNum = corpNum;

            // 공급자 종사업자 식별번호. 필요시 기재. 형식은 숫자 4자리.
            taxinvoice.invoicerTaxRegID = "";

            // [필수] 공급자 상호
            taxinvoice.invoicerCorpName = "공급자 상호";

            // [필수] 공급자 문서관리번호, 숫자, 영문, '-', '_' 조합으로 
            //  1~24자리까지 사업자번호별 중복없는 고유번호 할당
            taxinvoice.invoicerMgtKey = "20181113122042";

            // [필수] 공급자 대표자 성명 
            taxinvoice.invoicerCEOName = "공급자 대표자 성명";

            // 공급자 주소 
            taxinvoice.invoicerAddr = "공급자 주소";

            // 공급자 종목
            taxinvoice.invoicerBizClass = "공급자 종목";

            // 공급자 업태 
            taxinvoice.invoicerBizType = "공급자 업태";

            // 공급자 담당자 성명 
            taxinvoice.invoicerContactName = "공급자 담당자명";

            // 공급자 담당자 메일주소 
            taxinvoice.invoicerEmail = "test@invoicer.com";

            // 공급자 담당자 연락처
            taxinvoice.invoicerTEL = "070-4304-2992";

            // 공급자 담당자 휴대폰번호 
            taxinvoice.invoicerHP = "010-1234-5678";

            // 발행시 알림문자 전송여부
            // - 공급받는자 담당자 휴대폰번호(invoiceeHP1)로 전송
            // - 전송시 포인트가 차감되며 전송실패하는 경우 포인트 환불처리
            taxinvoice.invoicerSMSSendYN = false;

            
            /*********************************************************************
             *                         공급받는자 정보                           *
             *********************************************************************/

            // [필수] 공급받는자 구분, {사업자, 개인, 외국인} 중 기재 
            taxinvoice.invoiceeType = "사업자";

            // [필수] 공급받는자 사업자번호, '-'제외 10자리
            taxinvoice.invoiceeCorpNum = "8888888888";

            // [필수] 공급받는자 상호
            taxinvoice.invoiceeCorpName = "공급받는자 상호";

            // [역발행시 필수] 공급받는자 문서관리번호, 숫자, 영문, '-', '_' 조합으로
            // 1~24자리까지 사업자번호별 중복없는 고유번호 할당
            taxinvoice.invoiceeMgtKey = "";

            // [필수] 공급받는자 대표자 성명 
            taxinvoice.invoiceeCEOName = "공급받는자 대표자 성명";

            // 공급받는자 주소 
            taxinvoice.invoiceeAddr = "공급받는자 주소";

            // 공급받는자 종목
            taxinvoice.invoiceeBizClass = "공급받는자 종목";

            // 공급받는자 업태 
            taxinvoice.invoiceeBizType = "공급받는자 업태";

            // 공급받는자 담당자 연락처
            taxinvoice.invoiceeTEL1 = "070-1234-1234";

            // 공급받는자 담당자명 
            taxinvoice.invoiceeContactName1 = "공급받는자 담당자명";

            // 공급받는자 담당자 메일주소 
            taxinvoice.invoiceeEmail1 = "test@invoicee.com";

            // 공급받는자 담당자 휴대폰번호 
            taxinvoice.invoiceeHP1 = "010-5678-1234";

            // 역발행시 알림문자 전송여부 
            taxinvoice.invoiceeSMSSendYN = false;


            /*********************************************************************
             *                          세금계산서 정보                          *
             *********************************************************************/

            // [필수] 공급가액 합계
            taxinvoice.supplyCostTotal = "100000";

            // [필수] 세액 합계
            taxinvoice.taxTotal = "10000";

            // [필수] 합계금액,  공급가액 합계 + 세액 합계
            taxinvoice.totalAmount = "110000";

            // 기재상 일련번호 항목 
            taxinvoice.serialNum = "";

            // 기재상 현금 항목 
            taxinvoice.cash = "";

            // 기재상 수표 항목
            taxinvoice.chkBill = "";

            // 기재상 어음 항목
            taxinvoice.note = "";

            // 기재상 외상미수금 항목
            taxinvoice.credit = "";

            // 기재상 비고 항목
            taxinvoice.remark1 = "비고1";
            taxinvoice.remark2 = "비고2";
            taxinvoice.remark3 = "비고3";

            // 기재상 권 항목, 최대값 32767
            taxinvoice.kwon = 1;

            // 기재상 호 항목, 최대값 32767
            taxinvoice.ho = 1;

            // 사업자등록증 이미지 첨부여부
            taxinvoice.businessLicenseYN = false;

            // 통장사본 이미지 첨부여부 
            taxinvoice.bankBookYN = false;


            /**************************************************************************
             *        수정세금계산서 정보 (수정세금계산서 작성시에만 기재             *
             * - 수정세금계산서 관련 정보는 연동매뉴얼 또는 개발가이드 링크 참조      *
             * - [참고] 수정세금계산서 작성방법 안내 - http://blog.linkhub.co.kr/650  *
             *************************************************************************/

            // 수정사유코드, 1~6까지 선택기재.
            taxinvoice.modifyCode = null;

            // 수정세금계산서 작성시 원본세금계산서의 ItemKey기재
            // - 원본세금계산서의 ItemKey는 문서정보 (GetInfo API) 응답항목으로 확인할 수 있습니다.
            taxinvoice.originalTaxinvoiceKey = "";


            /********************************************************************************
             *                         상세항목(품목) 정보                                      *
             * - 상세항목 정보는 세금계산서 필수기재사항이 아니므로 작성하지 않더라도 세금계산서 발행이 가능합니다.  *
             * - 최대 99건까지 작성가능                                                          *
             ********************************************************************************/

            taxinvoice.detailList = new List<TaxinvoiceDetail>();

            TaxinvoiceDetail detail = new TaxinvoiceDetail
            {
                serialNum = 1, // 일련번호, 1부터 순차기재 
                purchaseDT = "20181113", // 거래일자
                itemName = "품목명", // 품목명 
                spec = "규격", // 규격
                qty = "1", // 수량
                unitCost = "50000", // 단가
                supplyCost = "50000", // 공급가액
                tax = "5000", // 세액
                remark = "품목비고" //비고
            };
            taxinvoice.detailList.Add(detail);

            detail = new TaxinvoiceDetail
            {
                serialNum = 2, // 일련번호, 1부터 순차기재 
                purchaseDT = "20181113", // 거래일자
                itemName = "품목명", // 품목명 
                spec = "규격", // 규격
                qty = "1", // 수량
                unitCost = "50000", // 단가
                supplyCost = "50000", // 공급가액
                tax = "5000", // 세액
                remark = "품목비고" //비고
            };
            taxinvoice.detailList.Add(detail);


            /**************************************************************
            *                           추가담당자 정보                       *  
            * - 세금계산서 발행안내 메일을 수신받을 공급받는자 담당자가 다수인 경우 담당자   *
            *   정보를 추가하여 발행안내메일을 다수에게 전송할 수 있습니다.              *
            * - 최대 5개까지 기재가능                                          *
            ***************************************************************/

            taxinvoice.addContactList = new List<TaxinvoiceAddContact>();

            TaxinvoiceAddContact addContact = new TaxinvoiceAddContact
            {
                serialNum = 1, // 일련번호, 1부터 순차기재
                email = "test1@invoicee.com", // 추가담당자 메일주소 
                contactName = "추가담당자명1" // 추가담당자 성명 
            };
            taxinvoice.addContactList.Add(addContact);

            addContact = new TaxinvoiceAddContact
            {
                serialNum = 2, // 일련번호, 1부터 순차기재
                email = "test2@invoicee.com", // 추가담당자 메일주소 
                contactName = "추가담당자명2" // 추가담당자 성명 
            };
            taxinvoice.addContactList.Add(addContact);

            
            // 거래명세서 동시작성여부
            bool writeSpecification = false;

            // 지연발행 강제여부
            // - 발행마감일이 지난 세금계산서를 발행하는 경우, 가산세가 부과될 수 있습니다.
            // - 가산세가 부과되더라도 발행을 해야하는 경우에는 forceIssue의 값을 true로 선언하여 호출하시면 됩니다.
            bool forceIssue = false;

            // 거래명세서 동시작성시 거래명세서 관리번호, 미기재시 세금계산서 관리번호로 자동작성
            string dealInvoiceMgtKey = "";

            // 메모
            string memo = "";

            // 발행안내메일 제목, 미기재시 기본양식으로 전송됨
            string emailSubject = "";

            try
            {
                var response = _taxinvoiceService.RegistIssue(corpNum, taxinvoice, writeSpecification, forceIssue,
                    dealInvoiceMgtKey, memo, emailSubject);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 1건의 세금계산서를 [임시저장]합니다.
         * - 세금계산서 임시저장(Register API) 호출후에는 발행(Issue API)을 호출해야만 국세청으로 전송됩니다.
         * - 임시저장과 발행을 한번의 호출로 처리하는 즉시발행(RegistIssue API) 프로세스 연동을 권장합니다.
         * - 세금계산서 항목별 정보는 "[전자세금계산서 API 연동매뉴얼] > 4.1. (세금)계산서구성"을 참조하시기 바랍니다.
         */
        public IActionResult Register()
        {
            // 세금계산서 정보 객체 
            Taxinvoice taxinvoice = new Taxinvoice();

            // [필수] 기재상 작성일자, 날짜형식(yyyyMMdd)
            taxinvoice.writeDate = "20181113";

            // [필수] 과금방향, {정과금, 역과금}중 선택
            // - 정과금(공급자과금), 역과금(공급받는자과금)
            // - 역과금은 역발행 세금계산서를 발행하는 경우만 가능
            taxinvoice.chargeDirection = "정과금";

            // [필수] 발행형태, {정발행, 역발행, 위수탁} 중 기재 
            taxinvoice.issueType = "정발행";

            // [필수] {영수, 청구} 중 기재
            taxinvoice.purposeType = "영수";

            // [필수] 발행시점, {직접발행, 승인시자동발행} 중 기재 
            // - {승인시자동발행}은 발행예정 프로세스에서만 이용가능
            taxinvoice.issueTiming = "직접발행";

            // [필수] 과세형태, {과세, 영세, 면세} 중 기재
            taxinvoice.taxType = "과세";


            /*****************************************************************
             *                         공급자 정보                           *
             *****************************************************************/

            // [필수] 공급자 사업자번호, '-' 제외 10자리
            taxinvoice.invoicerCorpNum = corpNum;

            // 공급자 종사업자 식별번호. 필요시 기재. 형식은 숫자 4자리.
            taxinvoice.invoicerTaxRegID = "";

            // [필수] 공급자 상호
            taxinvoice.invoicerCorpName = "공급자 상호";

            // [필수] 공급자 문서관리번호, 숫자, 영문, '-', '_' 조합으로 
            //  1~24자리까지 사업자번호별 중복없는 고유번호 할당
            taxinvoice.invoicerMgtKey = "20181113122042";

            // [필수] 공급자 대표자 성명 
            taxinvoice.invoicerCEOName = "공급자 대표자 성명";

            // 공급자 주소 
            taxinvoice.invoicerAddr = "공급자 주소";

            // 공급자 종목
            taxinvoice.invoicerBizClass = "공급자 종목";

            // 공급자 업태 
            taxinvoice.invoicerBizType = "공급자 업태";

            // 공급자 담당자 성명 
            taxinvoice.invoicerContactName = "공급자 담당자명";

            // 공급자 담당자 메일주소 
            taxinvoice.invoicerEmail = "test@invoicer.com";

            // 공급자 담당자 연락처
            taxinvoice.invoicerTEL = "070-4304-2992";

            // 공급자 담당자 휴대폰번호 
            taxinvoice.invoicerHP = "010-1234-5678";

            // 발행시 알림문자 전송여부
            // - 공급받는자 담당자 휴대폰번호(invoiceeHP1)로 전송
            // - 전송시 포인트가 차감되며 전송실패하는 경우 포인트 환불처리
            taxinvoice.invoicerSMSSendYN = false;


            /*********************************************************************
             *                         공급받는자 정보                           *
             *********************************************************************/

            // [필수] 공급받는자 구분, {사업자, 개인, 외국인} 중 기재 
            taxinvoice.invoiceeType = "사업자";

            // [필수] 공급받는자 사업자번호, '-'제외 10자리
            taxinvoice.invoiceeCorpNum = "8888888888";

            // [필수] 공급받는자 상호
            taxinvoice.invoiceeCorpName = "공급받는자 상호";

            // [역발행시 필수] 공급받는자 문서관리번호, 숫자, 영문, '-', '_' 조합으로
            // 1~24자리까지 사업자번호별 중복없는 고유번호 할당
            taxinvoice.invoiceeMgtKey = "";

            // [필수] 공급받는자 대표자 성명 
            taxinvoice.invoiceeCEOName = "공급받는자 대표자 성명";

            // 공급받는자 주소 
            taxinvoice.invoiceeAddr = "공급받는자 주소";

            // 공급받는자 종목
            taxinvoice.invoiceeBizClass = "공급받는자 종목";

            // 공급받는자 업태 
            taxinvoice.invoiceeBizType = "공급받는자 업태";

            // 공급받는자 담당자 연락처
            taxinvoice.invoiceeTEL1 = "070-1234-1234";

            // 공급받는자 담당자명 
            taxinvoice.invoiceeContactName1 = "공급받는자 담당자명";

            // 공급받는자 담당자 메일주소 
            taxinvoice.invoiceeEmail1 = "test@invoicee.com";

            // 공급받는자 담당자 휴대폰번호 
            taxinvoice.invoiceeHP1 = "010-5678-1234";

            // 역발행시 알림문자 전송여부 
            taxinvoice.invoiceeSMSSendYN = false;


            /*********************************************************************
             *                          세금계산서 정보                          *
             *********************************************************************/

            // [필수] 공급가액 합계
            taxinvoice.supplyCostTotal = "100000";

            // [필수] 세액 합계
            taxinvoice.taxTotal = "10000";

            // [필수] 합계금액,  공급가액 합계 + 세액 합계
            taxinvoice.totalAmount = "110000";

            // 기재상 일련번호 항목 
            taxinvoice.serialNum = "";

            // 기재상 현금 항목 
            taxinvoice.cash = "";

            // 기재상 수표 항목
            taxinvoice.chkBill = "";

            // 기재상 어음 항목
            taxinvoice.note = "";

            // 기재상 외상미수금 항목
            taxinvoice.credit = "";

            // 기재상 비고 항목
            taxinvoice.remark1 = "비고1";
            taxinvoice.remark2 = "비고2";
            taxinvoice.remark3 = "비고3";

            // 기재상 권 항목, 최대값 32767
            taxinvoice.kwon = 1;

            // 기재상 호 항목, 최대값 32767
            taxinvoice.ho = 1;
            // 사업자등록증 이미지 첨부여부
            taxinvoice.businessLicenseYN = false;

            // 통장사본 이미지 첨부여부 
            taxinvoice.bankBookYN = false;

            /**************************************************************************
             *        수정세금계산서 정보 (수정세금계산서 작성시에만 기재             *
             * - 수정세금계산서 관련 정보는 연동매뉴얼 또는 개발가이드 링크 참조      *
             * - [참고] 수정세금계산서 작성방법 안내 - http://blog.linkhub.co.kr/650  *
             *************************************************************************/

            // 수정사유코드, 1~6까지 선택기재.
            taxinvoice.modifyCode = null;

            // 수정세금계산서 작성시 원본세금계산서의 ItemKey기재
            // - 원본세금계산서의 ItemKey는 문서정보 (GetInfo API) 응답항목으로 확인할 수 있습니다.
            taxinvoice.originalTaxinvoiceKey = "";


            /********************************************************************************
             *                         상세항목(품목) 정보                                      *
             * - 상세항목 정보는 세금계산서 필수기재사항이 아니므로 작성하지 않더라도 세금계산서 발행이 가능합니다.  *
             * - 최대 99건까지 작성가능                                                          *
             ********************************************************************************/

            taxinvoice.detailList = new List<TaxinvoiceDetail>();

            TaxinvoiceDetail detail = new TaxinvoiceDetail
            {
                serialNum = 1, // 일련번호, 1부터 순차기재 
                purchaseDT = "20181113", // 거래일자
                itemName = "품목명", // 품목명 
                spec = "규격", // 규격
                qty = "1", // 수량
                unitCost = "50000", // 단가
                supplyCost = "50000", // 공급가액
                tax = "5000", // 세액
                remark = "품목비고" //비고
            };
            taxinvoice.detailList.Add(detail);

            detail = new TaxinvoiceDetail
            {
                serialNum = 2, // 일련번호, 1부터 순차기재 
                purchaseDT = "20181113", // 거래일자
                itemName = "품목명", // 품목명 
                spec = "규격", // 규격
                qty = "1", // 수량
                unitCost = "50000", // 단가
                supplyCost = "50000", // 공급가액
                tax = "5000", // 세액
                remark = "품목비고" //비고
            };
            taxinvoice.detailList.Add(detail);


            /**************************************************************
            *                           추가담당자 정보                       *  
            * - 세금계산서 발행안내 메일을 수신받을 공급받는자 담당자가 다수인 경우 담당자   *
            *   정보를 추가하여 발행안내메일을 다수에게 전송할 수 있습니다.              *
            * - 최대 5개까지 기재가능                                          *
            ***************************************************************/

            taxinvoice.addContactList = new List<TaxinvoiceAddContact>();

            TaxinvoiceAddContact addContact = new TaxinvoiceAddContact
            {
                serialNum = 1, // 일련번호, 1부터 순차기재
                email = "test1@invoicee.com", // 추가담당자 메일주소 
                contactName = "추가담당자명1" // 추가담당자 성명 
            };
            taxinvoice.addContactList.Add(addContact);

            addContact = new TaxinvoiceAddContact
            {
                serialNum = 2, // 일련번호, 1부터 순차기재
                email = "test2@invoicee.com", // 추가담당자 메일주소 
                contactName = "추가담당자명2" // 추가담당자 성명 
            };
            taxinvoice.addContactList.Add(addContact);


            // 거래명세서 동시작성여부
            bool writeSpecification = false;

            // 거래명세서 동시작성시 거래명세서 관리번호, 미기재시 세금계산서 관리번호로 자동작성
            string dealInvoiceMgtKey = "";

            try
            {
                var response = _taxinvoiceService.Register(corpNum, taxinvoice, writeSpecification, dealInvoiceMgtKey);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * [임시저장] 상태의 세금계산서의 항목을 [수정]합니다.
         * - 세금계산서 항목별 정보는 "[전자세금계산서 API 연동매뉴얼] > 4.1. (세금)계산서구성"을 참조하시기 바랍니다.
         */
        public IActionResult Update()
        {
            // 수정할 세금계산서 문서관리번호
            string mgtKey = "20181030-002";

            // 세금계산서유형, SELL(매출), BUY(매입), TRUSTEE(위수탁)
            MgtKeyType mgtKeyType = MgtKeyType.SELL;

            
            // 세금계산서 정보 객체 
            Taxinvoice taxinvoice = new Taxinvoice();

            // [필수] 기재상 작성일자, 날짜형식(yyyyMMdd)
            taxinvoice.writeDate = "20181113";

            // [필수] 과금방향, {정과금, 역과금}중 선택
            // - 정과금(공급자과금), 역과금(공급받는자과금)
            // - 역과금은 역발행 세금계산서를 발행하는 경우만 가능
            taxinvoice.chargeDirection = "정과금";

            // [필수] 발행형태, {정발행, 역발행, 위수탁} 중 기재 
            taxinvoice.issueType = "정발행";

            // [필수] {영수, 청구} 중 기재
            taxinvoice.purposeType = "영수";

            // [필수] 발행시점, {직접발행, 승인시자동발행} 중 기재 
            // - {승인시자동발행}은 발행예정 프로세스에서만 이용가능
            taxinvoice.issueTiming = "직접발행";

            // [필수] 과세형태, {과세, 영세, 면세} 중 기재
            taxinvoice.taxType = "과세";

            
            /*****************************************************************
             *                         공급자 정보                           *
             *****************************************************************/

            // [필수] 공급자 사업자번호, '-' 제외 10자리
            taxinvoice.invoicerCorpNum = corpNum;

            // 공급자 종사업자 식별번호. 필요시 기재. 형식은 숫자 4자리.
            taxinvoice.invoicerTaxRegID = "";

            // [필수] 공급자 상호
            taxinvoice.invoicerCorpName = "공급자 상호";

            // [필수] 공급자 문서관리번호, 숫자, 영문, '-', '_' 조합으로 
            //  1~24자리까지 사업자번호별 중복없는 고유번호 할당
            taxinvoice.invoicerMgtKey = "20181113122042";

            // [필수] 공급자 대표자 성명 
            taxinvoice.invoicerCEOName = "공급자 대표자 성명";

            // 공급자 주소 
            taxinvoice.invoicerAddr = "공급자 주소";

            // 공급자 종목
            taxinvoice.invoicerBizClass = "공급자 종목";

            // 공급자 업태 
            taxinvoice.invoicerBizType = "공급자 업태";

            // 공급자 담당자 성명 
            taxinvoice.invoicerContactName = "공급자 담당자명";

            // 공급자 담당자 메일주소 
            taxinvoice.invoicerEmail = "test@invoicer.com";

            // 공급자 담당자 연락처
            taxinvoice.invoicerTEL = "070-4304-2992";

            // 공급자 담당자 휴대폰번호 
            taxinvoice.invoicerHP = "010-1234-5678";

            // 발행시 알림문자 전송여부
            // - 공급받는자 담당자 휴대폰번호(invoiceeHP1)로 전송
            // - 전송시 포인트가 차감되며 전송실패하는 경우 포인트 환불처리
            taxinvoice.invoicerSMSSendYN = false;


            /*********************************************************************
             *                         공급받는자 정보                           *
             *********************************************************************/

            // [필수] 공급받는자 구분, {사업자, 개인, 외국인} 중 기재 
            taxinvoice.invoiceeType = "사업자";

            // [필수] 공급받는자 사업자번호, '-'제외 10자리
            taxinvoice.invoiceeCorpNum = "8888888888";

            // [필수] 공급받는자 상호
            taxinvoice.invoiceeCorpName = "공급받는자 상호";

            // [역발행시 필수] 공급받는자 문서관리번호, 숫자, 영문, '-', '_' 조합으로
            // 1~24자리까지 사업자번호별 중복없는 고유번호 할당
            taxinvoice.invoiceeMgtKey = "";

            // [필수] 공급받는자 대표자 성명 
            taxinvoice.invoiceeCEOName = "공급받는자 대표자 성명";

            // 공급받는자 주소 
            taxinvoice.invoiceeAddr = "공급받는자 주소";

            // 공급받는자 종목
            taxinvoice.invoiceeBizClass = "공급받는자 종목";

            // 공급받는자 업태 
            taxinvoice.invoiceeBizType = "공급받는자 업태";

            // 공급받는자 담당자 연락처
            taxinvoice.invoiceeTEL1 = "070-1234-1234";

            // 공급받는자 담당자명 
            taxinvoice.invoiceeContactName1 = "공급받는자 담당자명";

            // 공급받는자 담당자 메일주소 
            taxinvoice.invoiceeEmail1 = "test@invoicee.com";

            // 공급받는자 담당자 휴대폰번호 
            taxinvoice.invoiceeHP1 = "010-5678-1234";

            // 역발행시 알림문자 전송여부 
            taxinvoice.invoiceeSMSSendYN = false;


            /*********************************************************************
             *                          세금계산서 정보                          *
             *********************************************************************/

            // [필수] 공급가액 합계
            taxinvoice.supplyCostTotal = "100000";

            // [필수] 세액 합계
            taxinvoice.taxTotal = "10000";

            // [필수] 합계금액,  공급가액 합계 + 세액 합계
            taxinvoice.totalAmount = "110000";

            // 기재상 일련번호 항목 
            taxinvoice.serialNum = "";

            // 기재상 현금 항목 
            taxinvoice.cash = "";

            // 기재상 수표 항목
            taxinvoice.chkBill = "";

            // 기재상 어음 항목
            taxinvoice.note = "";

            // 기재상 외상미수금 항목
            taxinvoice.credit = "";

            // 기재상 비고 항목
            taxinvoice.remark1 = "비고1";
            taxinvoice.remark2 = "비고2";
            taxinvoice.remark3 = "비고3";

            // 기재상 권 항목, 최대값 32767
            taxinvoice.kwon = 1;

            // 기재상 호 항목, 최대값 32767
            taxinvoice.ho = 1;

            // 사업자등록증 이미지 첨부여부
            taxinvoice.businessLicenseYN = false;

            // 통장사본 이미지 첨부여부 
            taxinvoice.bankBookYN = false;


            /**************************************************************************
             *        수정세금계산서 정보 (수정세금계산서 작성시에만 기재             *
             * - 수정세금계산서 관련 정보는 연동매뉴얼 또는 개발가이드 링크 참조      *
             * - [참고] 수정세금계산서 작성방법 안내 - http://blog.linkhub.co.kr/650  *
             *************************************************************************/

            // 수정사유코드, 1~6까지 선택기재.
            taxinvoice.modifyCode = null;

            // 수정세금계산서 작성시 원본세금계산서의 ItemKey기재
            // - 원본세금계산서의 ItemKey는 문서정보 (GetInfo API) 응답항목으로 확인할 수 있습니다.
            taxinvoice.originalTaxinvoiceKey = "";


            /********************************************************************************
             *                         상세항목(품목) 정보                                      *
             * - 상세항목 정보는 세금계산서 필수기재사항이 아니므로 작성하지 않더라도 세금계산서 발행이 가능합니다.  *
             * - 최대 99건까지 작성가능                                                          *
             ********************************************************************************/
            
            taxinvoice.detailList = new List<TaxinvoiceDetail>();

            TaxinvoiceDetail detail = new TaxinvoiceDetail
            {
                serialNum = 1,// 일련번호, 1부터 순차기재 
                purchaseDT = "20181113", // 거래일자
                itemName = "품목명",// 품목명 
                spec = "규격", // 규격
                qty = "1", // 수량
                unitCost = "50000",// 단가
                supplyCost = "50000", // 공급가액
                tax = "5000",// 세액
                remark = "품목비고" //비고
            };
            taxinvoice.detailList.Add(detail);

            detail = new TaxinvoiceDetail
            {
                serialNum = 2,// 일련번호, 1부터 순차기재 
                purchaseDT = "20181113", // 거래일자
                itemName = "품목명",// 품목명 
                spec = "규격", // 규격
                qty = "1", // 수량
                unitCost = "50000",// 단가
                supplyCost = "50000", // 공급가액
                tax = "5000",// 세액
                remark = "품목비고" //비고
            };
            taxinvoice.detailList.Add(detail);


            /**************************************************************
            *                           추가담당자 정보                       *  
            * - 세금계산서 발행안내 메일을 수신받을 공급받는자 담당자가 다수인 경우 담당자   *
            *   정보를 추가하여 발행안내메일을 다수에게 전송할 수 있습니다.              *
            * - 최대 5개까지 기재가능                                          *
            ***************************************************************/

            taxinvoice.addContactList = new List<TaxinvoiceAddContact>();

            TaxinvoiceAddContact addContact = new TaxinvoiceAddContact
            {
                serialNum = 1, // 일련번호, 1부터 순차기재
                email = "test1@invoicee.com", // 추가담당자 메일주소 
                contactName = "추가담당자명1"// 추가담당자 성명 
            };
            taxinvoice.addContactList.Add(addContact);

            addContact = new TaxinvoiceAddContact
            {
                serialNum = 2, // 일련번호, 1부터 순차기재
                email = "test2@invoicee.com", // 추가담당자 메일주소 
                contactName = "추가담당자명2"// 추가담당자 성명 
            };
            taxinvoice.addContactList.Add(addContact);

            try
            {
                var response = _taxinvoiceService.Update(corpNum, mgtKeyType, mgtKey, taxinvoice);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * [임시저장] 또는 [발행대기] 상태의 세금계산서를 [공급자]가 [발행]합니다.
         * - 세금계산서 항목별 정보는 "[전자세금계산서 API 연동매뉴얼] > 4.1. (세금)계산서구성"을 참조하시기 바랍니다.
         */
        public IActionResult Issue()
        {
            // 세금계산서유형, SELL(매출), BUY(매입), TRUSTEE(위수탁)
            MgtKeyType mgtKeyType = MgtKeyType.SELL;

            // 세금계산서 문서관리번호
            string mgtKey = "20181030-002";

            // 지연발행 강제여부, 기본값 - False
            // 발행마감일이 지난 세금계산서를 발행하는 경우, 가산세가 부과될 수 있습니다.
            // 지연발행 세금계산서를 신고해야 하는 경우 forceIssue 값을 True로 선언하여 발행(Issue API)을 호출할 수 있습니다.
            bool forceIssue = false;

            // 메모
            string memo = "발행메모";

            // 발행안내메일 제목, 미기재시 기본양식으로 전송됨
            string emailSubject = "";

            try
            {
                var response = _taxinvoiceService.Issue(corpNum, mgtKeyType, mgtKey, forceIssue, memo, emailSubject);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * [발행완료] 상태의 세금계산서를 [공급자]가 [발행취소]합니다.
         * - [발행취소]는 국세청 전송전에만 가능합니다.
         * - 발행취소된 세금계산서는 국세청에 전송되지 않습니다.
         * - 발행취소 세금계산서에 사용된 문서관리번호를 재사용 하기 위해서는 삭제(Delete API)를 호출하여 해당세금계산서를 삭제해야 합니다.
         */
        public IActionResult CancelIssue()
        {
            // 세금계산서유형, SELL(매출), BUY(매입), TRUSTEE(위수탁)
            MgtKeyType mgtKeyType = MgtKeyType.SELL;

            // 세금계산서 문서관리번호
            string mgtKey = "20181030-002";

            // 메모
            string memo = "발행 취소 메모";

            try
            {
                var response = _taxinvoiceService.CancelIssue(corpNum, mgtKeyType, mgtKey, memo);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*    
         * [임시저장] 상태의 세금계산서를 [공급자]가 [발행예정]합니다.
         * - 발행예정이란 공급자와 공급받는자 사이에 세금계산서 확인 후 발행하는 방법입니다.
         * - "[전자세금계산서 API 연동매뉴얼] > 1.3.1. 정발행 프로세스 흐름도 > 다. 임시저장 발행예정" 의 프로세스를 참조하시기 바랍니다.
         */
        public IActionResult Send()
        {
            // 세금계산서유형, SELL(매출), BUY(매입), TRUSTEE(위수탁)
            MgtKeyType mgtKeyType = MgtKeyType.SELL;

            // 세금계산서 문서관리번호
            string mgtKey = "20181113135305";

            //메모
            string memo = "발행예정 메모";

            //발행예정 메일제목, 공백으로 처리시 기본메일 제목으로 전송
            string emailSubject = "";

            try
            {
                var response = _taxinvoiceService.Send(corpNum, mgtKeyType, mgtKey, memo, emailSubject);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * [발행예정] 세금계산서를 [공급자]가 [취소]합니다.
         * - [취소]된 세금계산서를 삭제(Delete API)하면 등록된 문서관리번호를 재사용할 수 있습니다.
         */
        public IActionResult CancelSend()
        {
            // 세금계산서유형, SELL(매출), BUY(매입), TRUSTEE(위수탁)
            MgtKeyType mgtKeyType = MgtKeyType.SELL;

            // 세금계산서 문서관리번호
            string mgtKey = "20181113135305";

            //메모
            string memo = "발행예정 취소 메모";

            try
            {
                var response = _taxinvoiceService.CancelSend(corpNum, mgtKeyType, mgtKey, memo);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * [발행예정] 세금계산서를 [공급받는자]가 [승인]합니다.
         */
        public IActionResult Accept()
        {
            // 세금계산서유형, SELL(매출), BUY(매입), TRUSTEE(위수탁)
            MgtKeyType mgtKeyType = MgtKeyType.BUY;

            // 세금계산서 문서관리번호
            string mgtKey = "20181025-abc";

            //메모
            string memo = "발행예정 승인 메모";

            try
            {
                var response = _taxinvoiceService.Accept(corpNum, mgtKeyType, mgtKey, memo);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * [발행예정] 세금계산서를 [공급받는자]가 [거부]합니다.
         * - [거부]처리된 세금계산서를 삭제(Delete API)하면 등록된 문서관리번호를 재사용할 수 있습니다.
         */
        public IActionResult Deny()
        {
            // 세금계산서유형, SELL(매출), BUY(매입), TRUSTEE(위수탁)
            MgtKeyType mgtKeyType = MgtKeyType.BUY;

            // 세금계산서 문서관리번호
            string mgtKey = "20181016_001";

            //메모
            string memo = "발행예정 거부 메모";

            try
            {
                var response = _taxinvoiceService.Deny(corpNum, mgtKeyType, mgtKey, memo);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 1건의 전자세금계산서를 [삭제]합니다.
         * - 세금계산서를 삭제해야만 문서관리번호(mgtKey)를 재사용할 수 있습니다.
         * - 삭제가능한 문서 상태 : [임시저장], [발행취소], [발행예정 취소], [발행예정 거부]
         */
        public IActionResult Delete()
        {
            // 세금계산서유형, SELL(매출), BUY(매입), TRUSTEE(위수탁)
            MgtKeyType mgtKeyType = MgtKeyType.SELL;

            // 세금계산서 문서관리번호
            string mgtKey = "20181025-100";

            try
            {
                var response = _taxinvoiceService.Delete(corpNum, mgtKeyType, mgtKey);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * [공급받는자]가 공급자에게 1건의 역발행 세금계산서를 [요청]합니다.
         * - 역발행 세금계산서 프로세스를 구현하기 위해서는 공급자/공급받는자가 모두 팝빌에 회원이여야 합니다.
         * - 역발행 요청후 공급자가 [발행] 처리시 포인트가 차감되며 역발행 세금계산서 항목중 과금방향(ChargeDirection)에 기재한 값에 따라
         *   정과금(공급자과금) 또는 역과금(공급받는자과금) 처리됩니다.
         */
        public IActionResult Request()
        {
            // 세금계산서유형, SELL(매출), BUY(매입), TRUSTEE(위수탁)
            MgtKeyType mgtKeyType = MgtKeyType.BUY;

            // 세금계산서 문서관리번호
            string mgtKey = "nsmbr37hwy";

            //메모
            string memo = "역발행요청 메모";

            try
            {
                var response = _taxinvoiceService.Request(corpNum, mgtKeyType, mgtKey, memo);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 역발행 세금계산서를 [공급받는자]가 [취소]합니다.
         * - [취소]한 세금계산서의 문서관리번호를 재사용하기 위해서는 삭제 (Delete API)를 호출해야 합니다.
         */
        public IActionResult CancelRequest()
        {
            // 세금계산서유형, SELL(매출), BUY(매입), TRUSTEE(위수탁)
            MgtKeyType mgtKeyType = MgtKeyType.BUY;

            // 세금계산서 문서관리번호
            string mgtKey = "nsmbr37hwy";

            //메모
            string memo = "역발행 취소 메모";

            try
            {
                var response = _taxinvoiceService.CancelRequest(corpNum, mgtKeyType, mgtKey, memo);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 공급받는자에게 요청받은 역발행 세금계산서를 [공급자]가 [거부]합니다.
         * - 세금계산서의 문서관리번호를 재사용하기 위해서는 삭제 (Delete API)를 호출하여 [삭제] 처리해야 합니다.
         */
        public IActionResult Refuse()
        {
            // 세금계산서유형, SELL(매출), BUY(매입), TRUSTEE(위수탁)
            MgtKeyType mgtKeyType = MgtKeyType.SELL;

            // 세금계산서 문서관리번호
            string mgtKey = "20181030-r002";

            //메모
            string memo = "역발행 거부 메모";

            try
            {
                var response = _taxinvoiceService.Refuse(corpNum, mgtKeyType, mgtKey, memo);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * [발행완료] 상태의 세금계산서를 국세청으로 [즉시전송]합니다.
         * - 국세청 즉시전송을 호출하지 않은 세금계산서는 발행일 기준 익일 오후 3시에 팝빌 시스템에서 일괄적으로 국세청으로 전송합니다.
         * - 익일전송시 전송일이 법정공휴일인 경우 다음 영업일에 전송됩니다.
         * - 국세청 전송에 관한 사항은 "[전자세금계산서 API 연동매뉴얼] > 1.4 국세청 전송 정책" 을 참조하시기 바랍니다.
         */
        public IActionResult SendToNTS()
        {
            // 세금계산서유형, SELL(매출), BUY(매입), TRUSTEE(위수탁)
            MgtKeyType mgtKeyType = MgtKeyType.SELL;

            // 세금계산서 문서관리번호
            string mgtKey = "20181112-b1";

            try
            {
                var response = _taxinvoiceService.SendToNTS(corpNum, mgtKeyType, mgtKey);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        #endregion

        #region 세금계산서 정보확인

        /*
         * 1건의 세금계산서 상태/요약 정보를 확인합니다.
         * - 세금계산서 상태정보(GetInfo API) 응답항목에 대한 자세한 정보는
         *   "[전자세금계산서 API 연동매뉴얼] > 4.2. (세금)계산서 상태정보 구성" 을 참조하시기 바랍니다.
         */
        public IActionResult GetInfo()
        {
            // 세금계산서유형, SELL(매출), BUY(매입), TRUSTEE(위수탁)
            MgtKeyType mgtKeyType = MgtKeyType.SELL;

            // 세금계산서 문서관리번호
            string mgtKey = "20181030-001";

            try
            {
                var response = _taxinvoiceService.GetInfo(corpNum, mgtKeyType, mgtKey);
                return View("GetInfo", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 대량의 세금계산서 상태/요약 정보를 확인합니다. (최대 1000건)
         * - 세금계산서 상태정보(GetInfos API) 응답항목에 대한 자세한 정보는
         *   "[전자세금계산서 API 연동매뉴얼]  > 4.2. (세금)계산서 상태정보 구성" 을 참조하시기 바랍니다.
         */
        public IActionResult GetInfos()
        {
            // 세금계산서유형, SELL(매출), BUY(매입), TRUSTEE(위수탁)
            MgtKeyType mgtKeyType = MgtKeyType.SELL;

            // 조회할 세금계산서 문서관리번호 배열, (최대 1000건)
            List<string> MgtKeyList = new List<string> {"20161221-03", "20161221-02", "20161221-01"};

            try
            {
                var response = _taxinvoiceService.GetInfos(corpNum, mgtKeyType, MgtKeyList);
                return View("GetInfos", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 1건의 세금계산서 상세정보를 확인합니다.
         * - 응답항목에 대한 자세한 사항은 "[전자세금계산서 API 연동매뉴얼] > 4.1 (세금)계산서 구성" 을 참조하시기 바랍니다.
         */
        public IActionResult GetDetailInfo()
        {
            // 세금계산서유형, SELL(매출), BUY(매입), TRUSTEE(위수탁)
            MgtKeyType mgtKeyType = MgtKeyType.SELL;

            // 세금계산서 문서관리번호
            string mgtKey = "20181030-001";

            try
            {
                var response = _taxinvoiceService.GetDetailInfo(corpNum, mgtKeyType, mgtKey);
                return View("GetDetailInfo", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 검색조건을 사용하여 세금계산서 목록을 조회합니다.
         * - 응답항목에 대한 자세한 사항은 "[전자세금계산서 API 연동매뉴얼] > 4.2. (세금)계산서 상태정보 구성" 을 참조하시기 바랍니다.
         */
        public IActionResult Search()
        {
            // 세금계산서유형, SELL(매출), BUY(매입), TRUSTEE(위수탁)
            MgtKeyType mgtKeyType = MgtKeyType.SELL;

            // [필수] 일자유형, R-등록일자, I-발행일자, W-작성일자 중 1개기입
            string DType = "W";

            // [필수] 시작일자, 날자형식(yyyyMMdd)
            string SDate = "20181001";

            // [필수] 종료일자, 날자형식(yyyyMMdd)
            string EDate = "20181031";

            // 상태코드 배열, 미기재시 전체 상태조회, 문서상태 값 3자리의 배열, 2,3번째 자리에 와일드카드 가능
            string[] state = new string[3];
            state[0] = "3**";
            state[1] = "4**";
            state[2] = "6**";

            // 문서유형 배열, N-일반세금계산서, M-수정세금계산서
            string[] Type = new string[2];
            Type[0] = "N";
            Type[1] = "M";

            // 과세형태 배열, T-과세, N-면세, Z-영세 
            string[] TaxType = new string[3];
            TaxType[0] = "T";
            TaxType[1] = "N";
            TaxType[2] = "Z";

            // 발행형태 배열, N-정발행, R-역발행, T-위수탁
            string[] IssueType = new string[3];
            IssueType[0] = "N";
            IssueType[1] = "R";
            IssueType[2] = "T";

            // 지연발행 여부, 미기재시 전체, true-지연발행분 조회, false-정상발행분 조회
            bool? LateOnly = null;

            // 종사업장 유무, 공백-전체조회, 0-종사업장 없는 문서 조회, 1-종사업장번호 조건에 따라 조회
            string TaxRegIDYN = "";

            // 종사업장번호 유형, S-공급자, B-공급받는자, T-수탁자
            string TaxRegIDType = "S";

            // 종사업장번호, 콤마(",")로 구분하여 구성 ex) "0001,1234"
            string TaxRegID = "";

            // 페이지번호
            int Page = 1;

            // 페이지당 검색개수, 최대 1000건
            int PerPage = 10;

            // 정렬방향, A-오름차순, D-내림차순
            string Order = "D";

            // 거래처 조회, 거래처 사업자등록번호 또는 상호명 기재, 미기재시 전체조회 
            string Qstring = "";

            // 일반/연동 문서구분, 공백-전체조회, 0-일반문서 조회, 1-연동문서 조회
            string InterOPYN = "";

            try
            {
                var response = _taxinvoiceService.Search(corpNum, mgtKeyType, DType, SDate, EDate, state, Type, TaxType,
                    IssueType, LateOnly, TaxRegIDYN, TaxRegIDType, TaxRegID, Page, PerPage, Order, Qstring, InterOPYN);
                return View("Search", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 세금계산서 상태 변경이력을 확인합니다.
         * - 상태 변경이력 확인(GetLogs API) 응답항목에 대한 자세한 정보는
         *   "[전자세금계산서 API 연동매뉴얼] > 3.6.4 상태 변경이력 확인" 을 참조하시기 바랍니다.
         */
        public IActionResult GetLogs()
        {
            // 세금계산서유형, SELL(매출), BUY(매입), TRUSTEE(위수탁)
            MgtKeyType mgtKeyType = MgtKeyType.SELL;

            // 세금계산서 문서관리번호
            string mgtKey = "20181030-001";

            try
            {
                var response = _taxinvoiceService.GetLogs(corpNum, mgtKeyType, mgtKey);
                return View("GetLogs", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 팝빌 전자세금계산서 문서함 팝업 URL을 반환합니다.
         * 반환된 URL은 보안정책에 따라 30초의 유효시간을 갖습니다.
         */
        public IActionResult GetURL()
        {
            // TOGO - TBOX(임시문서함), SBOX(매출문서함), PBOX(매입문서함), WRITE(매출문서작성)
            string TOGO = "SBOX";

            try
            {
                var result = _taxinvoiceService.GetURL(corpNum, TOGO);
                return View("Result", result);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        #endregion

        #region 세금계산서 보기/인쇄

        /*
         * 1건의 전자세금계산서 보기 팝업 URL을 반환합니다.
         * - 반환된 URL은 보안정책으로 인해 30초의 유효시간을 갖습니다.
         */
        public IActionResult GetPopUpURL()
        {
            // 세금계산서유형, SELL(매출), BUY(매입), TRUSTEE(위수탁)
            MgtKeyType mgtKeyType = MgtKeyType.SELL;

            // 세금계산서 문서관리번호
            string mgtKey = "20181030-001";

            try
            {
                var result = _taxinvoiceService.GetPopUpURL(corpNum, mgtKeyType, mgtKey);
                return View("Result", result);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 1건의 전자세금계산서 인쇄팝업 URL을 반환합니다.
         * - 반환된 URL은 보안정책으로 인해 30초의 유효시간을 갖습니다.
         */
        public IActionResult GetPrintURL()
        {
            // 세금계산서유형, SELL(매출), BUY(매입), TRUSTEE(위수탁)
            MgtKeyType mgtKeyType = MgtKeyType.SELL;

            // 세금계산서 문서관리번호
            string mgtKey = "20181030-001";

            try
            {
                var result = _taxinvoiceService.GetPrintURL(corpNum, mgtKeyType, mgtKey);
                return View("Result", result);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 세금계산서 인쇄(공급받는자) 팝업 URL을 반환합니다.
         * - URL 보안정책에 따라 반환된 URL은 30초의 유효시간을 갖습니다.
         */
        public IActionResult GetEPrintURL()
        {
            // 세금계산서유형, SELL(매출), BUY(매입), TRUSTEE(위수탁)
            MgtKeyType mgtKeyType = MgtKeyType.SELL;

            // 세금계산서 문서관리번호
            string mgtKey = "20181030-001";

            try
            {
                var result = _taxinvoiceService.GetEPrintURL(corpNum, mgtKeyType, mgtKey);
                return View("Result", result);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 대량의 세금계산서 인쇄팝업 URL을 반환합니다. (최대 100건)
         * - 보안정책으로 인해 반환된 URL의 유효시간은 30초입니다.
         */
        public IActionResult GetMassPrintURL()
        {
            // 세금계산서유형, SELL(매출), BUY(매입), TRUSTEE(위수탁)
            MgtKeyType mgtKeyType = MgtKeyType.SELL;

            // 조회할 세금계산서 문서관리번호 배열, (최대 100건)
            List<string> MgtKeyList = new List<string> {"20161221-03", "20161221-02", "20161221-01"};

            try
            {
                var result = _taxinvoiceService.GetMassPrintURL(corpNum, mgtKeyType, MgtKeyList);
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
            // 세금계산서유형, SELL(매출), BUY(매입), TRUSTEE(위수탁)
            MgtKeyType mgtKeyType = MgtKeyType.SELL;

            // 세금계산서 문서관리번호
            string mgtKey = "20181030-001";

            try
            {
                var result = _taxinvoiceService.GetMailURL(corpNum, mgtKeyType, mgtKey);
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
                var result = _taxinvoiceService.GetAccessURL(corpNum, userID);
                return View("Result", result);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 인감 및 첨부문서 등록 URL을 반환합니다.
         * - 반환된 URL의 유지시간은 30초이며, 제한된 시간 이후에는 정상적으로 처리되지 않습니다.
         */
        public IActionResult GetSealURL()
        {
            try
            {
                var result = _taxinvoiceService.GetSealURL(corpNum, userID);
                return View("Result", result);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 세금계산서에 첨부파일을 등록합니다.
         * - [임시저장] 상태의 세금계산서만 파일을 첨부할수 있습니다.
         * - 첨부파일은 최대 5개까지 등록할 수 있습니다.
         */
        public IActionResult AttachFile()
        {
            // 세금계산서유형, SELL(매출), BUY(매입), TRUSTEE(위수탁)
            MgtKeyType mgtKeyType = MgtKeyType.SELL;

            // 세금계산서 문서관리번호
            string mgtKey = "MP1540877740-333273";

            // 첨부파일 경로
            string filePath =
                "/Users/kimhyunjin/SDK/popbill.example.dotnetcore/TaxinvoiceExample/wwwroot/images/tax_image.png";

            try
            {
                var response = _taxinvoiceService.AttachFile(corpNum, mgtKeyType, mgtKey, filePath);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 세금계산서에 첨부된 파일을 삭제합니다.
         * - 파일을 식별하는 파일아이디는 첨부파일 목록(GetFileList API) 의 응답항목 중 파일아이디(AttachedFile) 값을 통해 확인할 수 있습니다.
         */
        public IActionResult DeleteFile()
        {
            // 세금계산서유형, SELL(매출), BUY(매입), TRUSTEE(위수탁)
            MgtKeyType mgtKeyType = MgtKeyType.SELL;

            // 세금계산서 문서관리번호
            string mgtKey = "MP1540877740-333273";

            // 파일아이디, 첨부파일 목록(GetFileList API) 의 응답항목 중 파일아이디(AttachedFile) 값
            string fileID = "F2A701E3-053B-40D8-AF28-FE1CBBE7FB53.PBF";

            try
            {
                var response = _taxinvoiceService.DeleteFile(corpNum, mgtKeyType, mgtKey, fileID);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 세금계산서 첨부파일 목록을 확인합니다.
         * - 응답항목 중 파일아이디(AttachedFile) 항목은 파일삭제(DeleteFile API) 호출시 이용할 수 있습니다.
         */
        public IActionResult GetFiles()
        {
            // 세금계산서유형, SELL(매출), BUY(매입), TRUSTEE(위수탁)
            MgtKeyType mgtKeyType = MgtKeyType.SELL;

            // 세금계산서 문서관리번호
            string mgtKey = "MP1540877740-333273";

            try
            {
                var response = _taxinvoiceService.GetFiles(corpNum, mgtKeyType, mgtKey);
                return View("GetFiles", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 세금계산서 발행안내 메일을 재전송합니다.
         */
        public IActionResult SendEmail()
        {
            // 세금계산서유형, SELL(매출), BUY(매입), TRUSTEE(위수탁)
            MgtKeyType mgtKeyType = MgtKeyType.SELL;

            // 세금계산서 문서관리번호
            string mgtKey = "20181030-001";

            // 수신메일주소
            string email = "test@test.com";

            try
            {
                var response = _taxinvoiceService.SendEmail(corpNum, mgtKeyType, mgtKey, email);
                return View("Response", response);
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
            // 세금계산서유형, SELL(매출), BUY(매입), TRUSTEE(위수탁)
            MgtKeyType mgtKeyType = MgtKeyType.SELL;

            // 세금계산서 문서관리번호
            string mgtKey = "20181030-001";

            // 발신번호
            string senderNum = "070-4304-2992";

            // 수신번호
            string receiverNum = "010-111-222";

            // 문자메시지 내용, 90byte 초과시 길이가 조정되어 전송됨
            string contents = "알림문자 전송내용, 90byte 초과된 내용은 삭제되어 전송됨";

            try
            {
                var response =
                    _taxinvoiceService.SendSMS(corpNum, mgtKeyType, mgtKey, senderNum, receiverNum, contents);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 전자세금계산서를 팩스전송합니다.
         * - 팩스 전송 요청시 포인트가 차감됩니다. (전송실패시 환불처리)
         * - 전송내역 확인은 "팝빌 로그인" > [문자 팩스] > [팩스] > [전송내역] 메뉴에서 전송결과를 확인할 수 있습니다.
         */
        public IActionResult SendFAX()
        {
            // 세금계산서유형, SELL(매출), BUY(매입), TRUSTEE(위수탁)
            MgtKeyType mgtKeyType = MgtKeyType.SELL;

            // 세금계산서 문서관리번호
            string mgtKey = "20181030-001";

            // 발신번호
            string senderNum = "070-4304-2992";

            // 수신팩스번호
            string receiverNum = "070-111-222";

            try
            {
                var response = _taxinvoiceService.SendFAX(corpNum, mgtKeyType, mgtKey, senderNum, receiverNum);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 1건의 전자명세서를 세금계산서에 첨부합니다.
         */
        public IActionResult AttachStatement()
        {
            // 세금계산서유형, SELL(매출), BUY(매입), TRUSTEE(위수탁)
            MgtKeyType mgtKeyType = MgtKeyType.SELL;

            // 세금계산서 문서관리번호
            string mgtKey = "20181030-001";

            // 첨부할 명세서 코드 - 121(거래명세서), 122(청구서), 123(견적서), 124(발주서), 125(입금표), 126(영수증)
            int docItemCode = 121;

            // 첨부할 명세서 관리번호
            string docMgtKey = "20181025-002";

            try
            {
                var response = _taxinvoiceService.AttachStatement(corpNum, mgtKeyType, mgtKey, docItemCode, docMgtKey);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 세금계산서에 첨부된 전자명세서 1건을 첨부해제합니다.
         */
        public IActionResult DetachStatement()
        {
            // 세금계산서유형, SELL(매출), BUY(매입), TRUSTEE(위수탁)
            MgtKeyType mgtKeyType = MgtKeyType.SELL;

            // 세금계산서 문서관리번호
            string mgtKey = "20181030-001";

            // 첨부할 명세서 코드 - 121(거래명세서), 122(청구서), 123(견적서), 124(발주서), 125(입금표), 126(영수증)
            int docItemCode = 121;

            // 첨부할 명세서 관리번호
            string docMgtKey = "20181025-002";

            try
            {
                var response = _taxinvoiceService.DetachStatement(corpNum, mgtKeyType, mgtKey, docItemCode, docMgtKey);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 대용량 연계사업자 유통메일주소 목록을 반환합니다.
         */
        public IActionResult GetEmailPublicKeys()
        {
            try
            {
                var response = _taxinvoiceService.GetEmailPublicKeys(corpNum);
                return View("GetEmailPublicKeys", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 팝빌사이트에서 작성된 세금계산서에 파트너 문서관리번호를 할당합니다.
         */
        public IActionResult AssignMgtKey()
        {
            // 세금계산서유형, SELL(매출), BUY(매입), TRUSTEE(위수탁)
            MgtKeyType mgtKeyType = MgtKeyType.SELL;

            // 세금계산서 아이템키, 목록조회(Search) API의 반환항목중 ItemKey 참조
            string itemKey = "018103016112000001";
            
            // 세금계산서에 할당할 문서관리번호
            string mgtKey = "20181030-itemkey";

            try
            {
                var response = _taxinvoiceService.AssignMgtKey(corpNum, mgtKeyType, itemKey, mgtKey);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 전자세금계산서 관련 메일전송 항목에 대한 전송여부를 목록으로 반환한다.
         */
        public IActionResult ListEmailConfig()
        {
            try
            {
                var response = _taxinvoiceService.ListEmailConfig(corpNum);
                return View("ListEmailConfig", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 전자세금계산서 관련 메일전송 항목에 대한 전송여부를 수정합니다.
         *
         * 메일전송유형
         * [정발행]
         * TAX_ISSUE : 공급받는자에게 전자세금계산서가 발행 되었음을 알려주는 메일입니다.
         * TAX_ISSUE_INVOICER : 공급자에게 전자세금계산서가 발행 되었음을 알려주는 메일입니다.
         * TAX_CHECK : 공급자에게 전자세금계산서가 수신확인 되었음을 알려주는 메일입니다.
         * TAX_CANCEL_ISSUE : 공급받는자에게 전자세금계산서가 발행취소 되었음을 알려주는 메일입니다.
         *
         * [발행예정]
         * TAX_SEND : 공급받는자에게 [발행예정] 세금계산서가 발송 되었음을 알려주는 메일입니다.
         * TAX_ACCEPT : 공급자에게 [발행예정] 세금계산서가 승인 되었음을 알려주는 메일입니다.
         * TAX_ACCEPT_ISSUE : 공급자에게 [발행예정] 세금계산서가 자동발행 되었음을 알려주는 메일입니다.
         * TAX_DENY : 공급자에게 [발행예정] 세금계산서가 거부 되었음을 알려주는 메일입니다.
         * TAX_CANCEL_SEND : 공급받는자에게 [발행예정] 세금계산서가 취소 되었음을 알려주는 메일입니다.
         *
         * [역발행]
         * TAX_REQUEST : 공급자에게 세금계산서를 전자서명 하여 발행을 요청하는 메일입니다.
         * TAX_CANCEL_REQUEST : 공급받는자에게 세금계산서가 취소 되었음을 알려주는 메일입니다.
         * TAX_REFUSE : 공급받는자에게 세금계산서가 거부 되었음을 알려주는 메일입니다.
         *
         * [위수탁발행]
         * TAX_TRUST_ISSUE : 공급받는자에게 전자세금계산서가 발행 되었음을 알려주는 메일입니다.
         * TAX_TRUST_ISSUE_TRUSTEE : 수탁자에게 전자세금계산서가 발행 되었음을 알려주는 메일입니다.
         * TAX_TRUST_ISSUE_INVOICER : 공급자에게 전자세금계산서가 발행 되었음을 알려주는 메일입니다.
         * TAX_TRUST_CANCEL_ISSUE : 공급받는자에게 전자세금계산서가 발행취소 되었음을 알려주는 메일입니다.
         * TAX_TRUST_CANCEL_ISSUE_INVOICER : 공급자에게 전자세금계산서가 발행취소 되었음을 알려주는 메일입니다.
         *
         * [위수탁 발행예정]
         * TAX_TRUST_SEND : 공급받는자에게 [발행예정] 세금계산서가 발송 되었음을 알려주는 메일입니다.
         * TAX_TRUST_ACCEPT : 수탁자에게 [발행예정] 세금계산서가 승인 되었음을 알려주는 메일입니다.
         * TAX_TRUST_ACCEPT_ISSUE : 수탁자에게 [발행예정] 세금계산서가 자동발행 되었음을 알려주는 메일입니다.
         * TAX_TRUST_DENY : 수탁자에게 [발행예정] 세금계산서가 거부 되었음을 알려주는 메일입니다.
         * TAX_TRUST_CANCEL_SEND : 공급받는자에게 [발행예정] 세금계산서가 취소 되었음을 알려주는 메일입니다.
         *
         * [처리결과]
         * TAX_CLOSEDOWN : 거래처의 휴폐업 여부를 확인하여 안내하는 메일입니다.
         * TAX_NTSFAIL_INVOICER : 전자세금계산서 국세청 전송실패를 안내하는 메일입니다.
         *
         * [정기발송]
         * TAX_SEND_INFO : 전월 귀속분 [매출 발행 대기] 세금계산서의 발행을 안내하는 메일입니다.
         * ETC_CERT_EXPIRATION : 팝빌에서 이용중인 공인인증서의 갱신을 안내하는 메일입니다.
         */
        public IActionResult UpdateEmailConfig()
        {
            //메일전송유형
            string emailType = "TAX_ISSUE";

            //전송여부 (true-전송, false-미전송)
            bool sendYN = true;

            try
            {
                var response = _taxinvoiceService.UpdateEmailConfig(corpNum, emailType, sendYN);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        #endregion

        #region 공인인증서 관리

        /*
        * 팝빌 회원의 공인인증서를 등록하는 팝업 URL을 반환합니다.
        * - 반환된 URL의 유지시간은 30초이며, 제한된 시간 이후에는 정상적으로 처리되지 않습니다.
        * - 팝빌에 등록된 공인인증서가 유효하지 않은 경우 (비밀번호 변경, 인증서 재발급/갱신, 만료일 경과)
        *   인증서를 재등록해야 정상적으로 전자세금계산서 발행이 가능합니다.
        */
        public IActionResult GetTaxCertURL()
        {
            try
            {
                var result = _taxinvoiceService.GetTaxCertURL(corpNum, userID);
                return View("Result", result);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 팝빌에 등록되어 있는 공인인증서의 만료일자를 확인합니다.
         * - 공인인증서가 갱신/재발급/비밀번호 변경이 되는 경우 해당 인증서를
         *   재등록 하셔야 정상적으로 세금계산서를 발행할 수 있습니다.
         */
        public IActionResult GetCertificateExpireDate()
        {
            try
            {
                var result = _taxinvoiceService.GetCertificateExpireDate(corpNum);
                return View("Result", result);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
        * 팝빌에 등록된 공인인증서의 유효성을 확인합니다.
        */
        public IActionResult CheckCertValidation()
        {
            try
            {
                var response = _taxinvoiceService.CheckCertValidation(corpNum);
                return View("Response", response);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        #endregion

        #region 포인트 관리

        /*
         * 연동회원 잔여포인트를 확인합니다.
         */
        public IActionResult GetBalance()
        {
            try
            {
                var result = _taxinvoiceService.GetBalance(corpNum);
                return View("Result", result);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 팝빌 연동회원의 포인트충전 팝업 URL을 반환합니다.
         * 반환된 URL의 유지시간은 30초이며, 제한된 시간 이후에는 정상적으로 처리되지 않습니다.
         */
        public IActionResult GetChargeURL()
        {
            try
            {
                var result = _taxinvoiceService.GetChargeURL(corpNum, userID);
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
                var result = _taxinvoiceService.GetPartnerBalance(corpNum);
                return View("Result", result);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 파트너 포인트 충전 팝업 URL을 반환합니다.
         * - 반환된 URL은 보안정책에 따라 30초의 유효시간을 갖습니다.
         */
        public IActionResult GetPartnerURL()
        {
            // CHRG 포인트충전 URL
            string TOGO = "CHRG";

            try
            {
                var result = _taxinvoiceService.GetPartnerURL(corpNum, TOGO);
                return View("Result", result);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 전자세금계산서 발행단가를 확인합니다.
         */
        public IActionResult GetUnitCost()
        {
            try
            {
                var result = _taxinvoiceService.GetUnitCost(corpNum);
                return View("Result", result);
            }
            catch (PopbillException pe)
            {
                return View("Exception", pe);
            }
        }

        /*
         * 전자세금계산서 API 서비스 과금정보를 확인합니다.
         */
        public IActionResult GetChargeInfo()
        {
            try
            {
                var response = _taxinvoiceService.GetChargeInfo(corpNum);
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

                var response = _taxinvoiceService.CheckIsMember(corpNum, linkID);
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
                var response = _taxinvoiceService.CheckID(checkID);
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
                var response = _taxinvoiceService.JoinMember(joinInfo);
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
                var response = _taxinvoiceService.GetCorpInfo(corpNum, userID);
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
                var response = _taxinvoiceService.UpdateCorpInfo(corpNum, corpInfo, userID);
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
                var response = _taxinvoiceService.RegistContact(corpNum, contactInfo, userID);
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
                var response = _taxinvoiceService.ListContact(corpNum, userID);
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
                var response = _taxinvoiceService.UpdateContact(corpNum, contactInfo, userID);
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