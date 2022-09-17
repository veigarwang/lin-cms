using System;
using System.Collections.Generic;
using FreeSql;
using LinCms.Common;
using LinCms.Data.Enums;
using LinCms.Entities;
using LinCms.Entities.Base;

namespace LinCms.FreeSql
{
    public static class CodeFirstExtension
    {
        public static ICodeFirst SeedData(this ICodeFirst @this)
        {
            @this
                .Entity<LinGroup>(e =>
                {
                    e.HasData(new List<LinGroup>()
                        {
                            new(LinGroup.Admin,"系统管理员",true),
                            new(LinGroup.CmsAdmin,"CMS管理员",true),
                            new(LinGroup.User,"普通用户",true)
                        });
                })
                .Entity<LinUser>(e =>
                {
                    e.HasData(new List<LinUser>()
                    {
                        new()
                        {
                            Nickname="系统管理员",
                            Username="admin",
                            Active=UserStatus.Active,
                            CreateTime=DateTime.Now,
                            IsDeleted=false,
                            Salt="9fd248c8-e9da-412f-bad9-aa5f7f1d7b80",
                            LinUserIdentitys=new List<LinUserIdentity>()
                            {
                               new(LinUserIdentity.Password,"admin","IWxIlqMAE3SU3JTogdDAJw==",DateTime.Now) //密码是 123qwe
                            },
                            LinUserGroups=new List<LinUserGroup>()
                            {
                                new(1,LinConsts.Group.Admin)
                            },
                        },
                        new()
                         {
                             Nickname="CMS管理员",
                             Username="CmsAdmin",
                             Active=UserStatus.Active,
                             CreateTime=DateTime.Now,
                             IsDeleted=false,
                             Salt="9fd248c8-e9da-412f-bad9-aa5f7f1d7b80",
                             LinUserIdentitys=new List<LinUserIdentity>()
                             {
                                 new(LinUserIdentity.Password,"CmsAdmin","IWxIlqMAE3SU3JTogdDAJw==",DateTime.Now)
                             },
                             LinUserGroups=new List<LinUserGroup>()
                             {
                                 new(2,LinConsts.Group.CmsAdmin)
                             },
                         }
                    });
                })
                .Entity<BaseType>(e =>
                {
                    e.HasData(new List<BaseType>()
                    {
                        new BaseType("Book.Type","书籍类别",1)
                        {
                            CreateTime=DateTime.Now,IsDeleted=false,CreateUserId = 1,
                            BaseItems=new List<BaseItem>()
                            {
                                new BaseItem("0","经典原著",1,1,true){CreateUserId = 1,CreateTime=DateTime.Now,IsDeleted=false},
                                new BaseItem("1","校注图解",2,1,true){CreateUserId = 1,CreateTime=DateTime.Now,IsDeleted=false},
                                new BaseItem("2","精美画册",3,1,true){CreateUserId = 1,CreateTime=DateTime.Now,IsDeleted=false},
                                new BaseItem("3","学术文章",4,1,true){CreateUserId = 1,CreateTime=DateTime.Now,IsDeleted=false},
                                new BaseItem("4","衍生小说",5,1,true){CreateUserId = 1,CreateTime=DateTime.Now,IsDeleted=false},
                            }
                        },
                        new BaseType("Author.Type","著者类型",2)
                        {
                            CreateTime=DateTime.Now,IsDeleted=false,CreateUserId = 1,
                            BaseItems=new List<BaseItem>()
                            {
                                new BaseItem("0","作者",1,2,true){CreateUserId = 1,CreateTime=DateTime.Now,IsDeleted=false},
                                new BaseItem("1","编者",2,2,true){CreateUserId = 1,CreateTime=DateTime.Now,IsDeleted=false},
                                new BaseItem("2","著者",3,2,true){CreateUserId = 1,CreateTime=DateTime.Now,IsDeleted=false},
                                new BaseItem("3","笺疏",4,2,true){CreateUserId = 1,CreateTime=DateTime.Now,IsDeleted=false},
                                new BaseItem("4","编著",5,2,true){CreateUserId = 1,CreateTime=DateTime.Now,IsDeleted=false},
                                new BaseItem("5","译注",6,2,true){CreateUserId = 1,CreateTime=DateTime.Now,IsDeleted=false},
                                new BaseItem("6","校注",7,2,true){CreateUserId = 1,CreateTime=DateTime.Now,IsDeleted=false},
                                new BaseItem("7","点校",8,2,true){CreateUserId = 1,CreateTime=DateTime.Now,IsDeleted=false},
                                new BaseItem("8","绘者",9,2,true){CreateUserId = 1,CreateTime=DateTime.Now,IsDeleted=false},
                            }
                        },
                        new BaseType("Encyclopedia.Type","词条类型",3)
                        {
                            CreateTime=DateTime.Now,IsDeleted=false,CreateUserId = 1,
                            BaseItems=new List<BaseItem>()
                            {
                                new BaseItem("0","山",1,3,true){CreateUserId = 1,CreateTime=DateTime.Now,IsDeleted=false},
                                new BaseItem("1","水",2,3,true){CreateUserId = 1,CreateTime=DateTime.Now,IsDeleted=false},
                                new BaseItem("2","草",3,3,true){CreateUserId = 1,CreateTime=DateTime.Now,IsDeleted=false},
                                new BaseItem("3","木",4,3,true){CreateUserId = 1,CreateTime=DateTime.Now,IsDeleted=false},
                                new BaseItem("4","虫",5,3,true){CreateUserId = 1,CreateTime=DateTime.Now,IsDeleted=false},
                                new BaseItem("5","魚",6,3,true){CreateUserId = 1,CreateTime=DateTime.Now,IsDeleted=false},
                                new BaseItem("6","鳥",7,3,true){CreateUserId = 1,CreateTime=DateTime.Now,IsDeleted=false},
                                new BaseItem("7","獸",8,3,true){CreateUserId = 1,CreateTime=DateTime.Now,IsDeleted=false},
                                new BaseItem("8","神",9,3,true){CreateUserId = 1,CreateTime=DateTime.Now,IsDeleted=false},
                                new BaseItem("9","怪",10,3,true){CreateUserId = 1,CreateTime=DateTime.Now,IsDeleted=false},
                                new BaseItem("10","国",11,3,true){CreateUserId = 1,CreateTime=DateTime.Now,IsDeleted=false},
                                new BaseItem("11","人",12,3,true){CreateUserId = 1,CreateTime=DateTime.Now,IsDeleted=false},
                                new BaseItem("12","器",13,3,true){CreateUserId = 1,CreateTime=DateTime.Now,IsDeleted=false},
                                new BaseItem("13","物",14,3,true){CreateUserId = 1,CreateTime=DateTime.Now,IsDeleted=false},
                                new BaseItem("14","金",15,3,true){CreateUserId = 1,CreateTime=DateTime.Now,IsDeleted=false},
                                new BaseItem("15","病",16,3,true){CreateUserId = 1,CreateTime=DateTime.Now,IsDeleted=false},
                            }
                        },
                         new BaseType("Sex","性别",4)
                         {
                             CreateTime=DateTime.Now,IsDeleted=false,CreateUserId = 1,
                             BaseItems=new List<BaseItem>()
                             {
                                 new BaseItem("0","男",1,4,true){CreateTime=DateTime.Now,IsDeleted=false},
                                 new BaseItem("1","女",2,4,true){CreateTime=DateTime.Now,IsDeleted=false},
                                 new BaseItem("2","保密",3,4,true){CreateTime=DateTime.Now,IsDeleted=false}
                             }
                         },
                         new BaseType("Article.Type","文章类型",5)
                        {
                            CreateTime=DateTime.Now,IsDeleted=false,CreateUserId = 1,
                            BaseItems=new List<BaseItem>()
                            {
                                new BaseItem("0","原创",1,5,true){CreateUserId = 1,CreateTime=DateTime.Now,IsDeleted=false},
                                new BaseItem("1","转载",2,5,true){CreateUserId = 1,CreateTime=DateTime.Now,IsDeleted=false},
                                new BaseItem("2","翻译",3,5,true){CreateUserId = 1,CreateTime=DateTime.Now,IsDeleted=false}
                            }
                        },
                    });
                });

            return @this;
        }
    }
}
