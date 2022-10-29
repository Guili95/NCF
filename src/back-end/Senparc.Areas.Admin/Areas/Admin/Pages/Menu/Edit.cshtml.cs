using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Senparc.Ncf.Core.Models;
using Senparc.Ncf.Core.Models.DataBaseModel;
using Senparc.Ncf.Service;
using Senparc.Xncf.AuditLog.Controllers;
using Senparc.Xncf.AuditLog.Domain.Services;

namespace Senparc.Areas.Admin.Areas.Admin.Pages
{
    public class MenuEditModel : BaseAdminPageModel
    {
        private readonly SysMenuService _sysMenuService;
        private readonly SysButtonService _sysButtonService;
        private readonly AuditLogService _auditLogService;

        public MenuEditModel(SysMenuService _sysMenuService, SysButtonService _sysButtonService, AuditLogService auditLogService)
        {
            CurrentMenu = "Menu";
            this._sysMenuService = _sysMenuService;
            this._sysButtonService = _sysButtonService;
            this._auditLogService = auditLogService;
        }

        [BindProperty(SupportsGet = true)]
        public string Id { get; set; }

        public SysMenuDto SysMenuDto { get; set; }

        public IEnumerable<SysButton> SysButtons { get; set; }

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrEmpty(Id))
            {
                var entity = await _sysMenuService.GetObjectAsync(_ => _.Id == Id);
                SysButtons = await _sysButtonService.GetFullListAsync(_ => _.MenuId == Id);

                SysMenuDto = _sysMenuService.Mapper.Map<SysMenuDto>(entity);
            }
            else
            {
                SysMenuDto = new SysMenuDto() { Visible = true };
                SysButtons = new List<SysButton>() { new SysButton() };
            }
        }

        public async Task<IActionResult> OnGetDetailAsync(string id)
        {
            var entity = await _sysMenuService.GetObjectAsync(_ => _.Id == Id);
            var sysMenuDto = _sysMenuService.Mapper.Map<SysMenuDto>(entity);
            var sysButtons = await _sysButtonService.GetFullListAsync(_ => _.MenuId == Id);
            return Ok( new{ sysMenuDto, sysButtons });
        }

        /// <summary>
        /// ��ȡ�˵�
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnGetMenuAsync()
        {
            //return Ok(await _sysMenuService.GetMenuDtoByCacheAsync(true));
            return Ok(await _sysMenuService.GetMenuDtoByDbAsync());
        }

        /// <summary>
        /// ��ȡ�˵��µİ�ť
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnGetButtonsAsync(string menuId)
        {
            return Ok(await _sysButtonService.GetSysButtonDtosAsync(menuId));
        }

        public async Task<IActionResult> OnPostDeleteButtonAsync(string buttonId)
        {
            if (string.IsNullOrEmpty(buttonId))
            {
                return Ok(false);
            }
            await _sysButtonService.DeleteObjectAsync(_ => _.Id == buttonId);
            _auditLogService.CreateAuditLogInfo(HttpContext.Session.GetString("userName"), IPAddressController.GetClientUserIp(HttpContext.Request.HttpContext), "ɾ���˵�", DateTime.Now.ToString());
            return Ok(true);
        }

        public async Task<IActionResult> OnPostDeleteAsync([FromBody]string[] ids)
        {
            var entity = await _sysMenuService.GetFullListAsync(_ => ids.Contains(_.Id) && _.IsLocked == false);
            var buttons = await _sysButtonService.GetFullListAsync(_ => ids.Contains(_.MenuId));

            await _sysButtonService.DeleteAllAsync(buttons);
            await _sysMenuService.DeleteAllAsync(entity);
            await _sysMenuService.RemoveMenuAsync();
            IEnumerable<string> unDeleteIds = ids.Except(entity.Select(_ => _.Id));
            _auditLogService.CreateAuditLogInfo(HttpContext.Session.GetString("userName"), IPAddressController.GetClientUserIp(HttpContext.Request.HttpContext), "ɾ���˵�", DateTime.Now.ToString());
            return Ok(unDeleteIds);
        }

        public async Task<IActionResult> OnPostAddMenuAsync([FromBody]SysMenuDto sysMenu)
        {
            if (string.IsNullOrEmpty(sysMenu.MenuName))
            {
                return Ok(false, "�˵����Ʋ���Ϊ��");
            }
            var entity = await _sysMenuService.CreateOrUpdateAsync(sysMenu);
            //_auditLogService.CreateAuditLogInfo(HttpContext.Session.GetString("userName"), IPAddressController.GetClientUserIp(HttpContext.Request.HttpContext), "��Ӳ˵�", DateTime.Now.ToString());
            return Ok(entity.Id);
        }

        public async Task<IActionResult> OnPostAsync([FromBody]SysMenuDto sysMenuDto)
        {
            if (!ModelState.IsValid)
            {
                return Ok(false, "ģ����֤δͨ��");
            }

            await _sysMenuService.CreateOrUpdateAsync(sysMenuDto);
            _auditLogService.CreateAuditLogInfo(HttpContext.Session.GetString("userName"), IPAddressController.GetClientUserIp(HttpContext.Request.HttpContext), "���/�༭�˵�", DateTime.Now.ToString());
            return Ok(new { sysMenuDto });
        }
    }
}