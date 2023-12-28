using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using IDZ.Models.Entities;
using IDZ.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace IDZ.Controllers
{
    public class mainController : Controller
    {
        //работа с клиентом : отображение, подробно, добавление, редактирование и удаление



        [HttpGet]
        [AllowAnonymous]
        [Authorize(Roles="Client")]
        public ActionResult ListOfClients()
        {
            List<Client> client = new List<Client>();
            using (var db = new Entities())
            {
                client = db.Clients.OrderByDescending(x => x.surname)
                                                .ThenBy(x => x.name)
                                                .ThenBy(x => x.patronymic).ToList();
            }
            return View(client);
        }


        [HttpGet]
        [Authorize(Roles = "Client,Master")]

        public ActionResult ClientDetails(Guid clientId)
        {
            using (var db = new Entities())
            {
                // Загрузка клиента из базы данных с проверкой на null
                var client = db.Clients.Include("ProvidedServices")
                                       .FirstOrDefault(c => c.clientID == clientId);
                if (client == null)
                {
                    return HttpNotFound();
                }

                // Создание модели представления 
                var model = new ClientDetailsViewModel
                {
                    Client = client,
                    ProvidedServices = client.providedServices.Select(ps => new ProvidedServiceInfo
                    {
                        ServiceName = ps.Masters_Services.Service.serviceName, // Имя услуги
                        ProvidedServiceDateTime = ps.providedServiceDateTime, // Время оказания услуги
                        ProvidedServiceStatus = ps.providedServiceStatus, // Статус оказанной услуги
                        MasterName = ps.Masters_Services.Master.name, // Имя мастера
                        MasterSurname = ps.Masters_Services.Master.surname, // Фамилия мастера
                        ServicePrice = ps.Masters_Services.Service.price
                    }).ToList()
                };

                return View(model);
            }
        }


        [HttpGet]
        [Authorize(Roles = "Master")]

        public ActionResult CreateClient()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateClient(ClientVM model)
        {
            if (ModelState.IsValid)
            {
                using(var db = new Entities())
                {
                    Client client = new Client
                    {
                        clientID = Guid.NewGuid(),
                        name = model.name,
                        surname = model.surname,
                        patronymic = model.patronymic,
                        phoneNumber = model.phoneNumber,
                        email = model.email
                    };
                    db.Clients.Add(client);
                    db.SaveChanges();
                    return RedirectToAction("ListOfClients");
                }
            }

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Master")]


        public ActionResult EditClient(Guid clientID)
        {
            ClientVM model;
            using(var db = new Entities())
            {
                Client c = db.Clients.Find(clientID);

                model = new ClientVM
                {
                    clientID = c.clientID,
                    name = c.name,
                    surname = c.surname,
                    patronymic = c.patronymic,
                    phoneNumber = c.phoneNumber,
                    email = c.email
                };
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditClient(ClientVM model)
        {
            if (ModelState.IsValid)
            {
                using(var db = new Entities())
                {
                    Client editedClient = new Client
                    {
                        clientID = model.clientID,
                        name = model.name,
                        surname = model.surname,
                        patronymic = model.patronymic,
                        phoneNumber = model.phoneNumber,
                        email = model.email
                    };

                    db.Clients.Attach(editedClient);
                    db.Entry(editedClient).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                return RedirectToAction("ListOfClients");
            }
            return View(model);
        }


        [HttpGet]
        [Authorize(Roles = "Master")]


        public ActionResult DeleteClient(Guid clientID)
        {
            Client clientToDelete;
            using(var db = new Entities())
            {
                clientToDelete = db.Clients.Find(clientID);
            }
            return View(clientToDelete);
        }


        [HttpPost,ActionName("DeleteClient")]

        public ActionResult DeleteClientConfirmed(Guid clientID)
        {
            using(var db = new Entities())
            {
                Client clientToDelete = db.Clients.Find(clientID);
                db.Clients.Remove(clientToDelete);
                db.SaveChanges();
            }

            return RedirectToAction("ListOfClients");

        }




        //providedService

        [HttpGet]
        [AllowAnonymous]
        [Authorize(Roles = "Master")]
        public ActionResult ListOfProvidedServices()
        {
            List<providedService> ps = new List<providedService>();
            var db = new Entities();
            ps = db.providedServices.OrderByDescending(x => x.masterID).ThenBy(x => x.clientID).ThenBy(x => x.providedServiceDateTime).ToList();
           
            return View(ps);
        }







        [HttpGet]
        [Authorize(Roles = "Master")]
        public ActionResult CreateProvidedService()
        {
            using(var db = new Entities())
            {

                DateTime psdt = DateTime.Now; // для передачи на форму текущей даты ,для упрощения выбора даты, не начиная с 0001 года

                var clients = db.Clients.ToList().Select(c => new SelectListItem
                {
                    Value = c.clientID.ToString(),
                    Text = $"ФИО: {c.surname} {c.name} {c.patronymic}   Номер телефона:{c.phoneNumber}"
                }).ToList();


                var masters = db.Masters.ToList().Select(m => new SelectListItem
                {
                    Value = m.masterID.ToString(),
                    Text = $"ФИО: {m.surname} {m.name} {m.patronymic}"
                }).ToList();

                var services = db.Services.ToList().Select(s => new SelectListItem
                {
                    Value = s.serviceName.ToString(),
                    Text = $"Название услуги: {s.serviceName}"
                }).ToList();



                var model = new providedServiceVM
                {
                    ClientsList = clients,
                    MastersList = masters,
                    ServicesList = services,
                    providedServiceDateTime = psdt,

                };

                return View(model);

            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateProvidedService(providedServiceVM model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new Entities())
                {
                    providedService ps = new providedService
                    {
                        providedServiceID = Guid.NewGuid(),
                        providedServiceDateTime = model.providedServiceDateTime,
                        providedServiceStatus = model.providedServiceStatus,
                        clientID = model.clientID,
                        masterID = model.masterID,
                        serviceName = model.serviceName
                              

                    };

                    db.providedServices.Add(ps);
                    db.SaveChanges();


                    return RedirectToAction("ListOfProvidedServices");
                } 
            }

           using(var db = new Entities())
            {
                var cl = db.Clients.ToList();
                var ms = db.Masters.ToList();
                var sv = db.Services.ToList();


                model.ClientsList = cl.Select(c => new SelectListItem
                {
                    Value = c.clientID.ToString(),
                    Text = $"ФИО: {c.surname} {c.name} {c.patronymic}   Номер телефона:{c.phoneNumber}"
                }).ToList();


              

                model.MastersList = ms.Select(m => new SelectListItem
                {
                    Value = m.masterID.ToString(),
                    Text = $"ФИО: {m.surname} {m.name} {m.patronymic}"
                }).ToList();


                model.ServicesList = sv.Select(s => new SelectListItem
                {
                    Value = s.serviceName.ToString(),
                    Text = $"Название услуги: {s.serviceName}"
                }).ToList();

            }

            ModelState.AddModelError("", "Ошибка");
            return View(model);
        }


        [HttpGet]
        [Authorize(Roles = "Master")]
        public ActionResult EditProvidedService(Guid providedServiceID)
        {
            providedServiceVM model;
            using (var db = new Entities())
            {
                providedService ps = db.providedServices.Find(providedServiceID);
                if (ps == null)
                {
                    return HttpNotFound();
                }

                var clients = db.Clients.ToList().Select(c => new SelectListItem
                {
                    Value = c.clientID.ToString(),
                    Text = $"ФИО: {c.surname} {c.name} {c.patronymic}   Номер телефона:{c.phoneNumber}"
                }).ToList();

                var masters = db.Masters.ToList().Select(m => new SelectListItem
                {
                    Value = m.masterID.ToString(),
                    Text = $"ФИО: {m.surname} {m.name} {m.patronymic}"
                }).ToList();

                var services = db.Services.ToList().Select(s => new SelectListItem
                {
                    Value = s.serviceName.ToString(),
                    Text = $"Название услуги: {s.serviceName}"
                }).ToList();

                // Необходимо заполнить модель данными из пс (providedService)
                model = new providedServiceVM
                {
                    providedServiceID = ps.providedServiceID,
                    clientID = ps.clientID,
                    masterID = ps.masterID,
                    serviceName = ps.serviceName,
                    providedServiceDateTime = ps.providedServiceDateTime,
                    providedServiceStatus = ps.providedServiceStatus,
                    ClientsList = clients,
                    MastersList = masters,
                    ServicesList = services
                };
            }

            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]

        public ActionResult EditProvidedService(providedServiceVM model)
        {
            if (ModelState.IsValid)
            {
                using(var db=  new Entities())
                {
                    providedService editedService = new providedService
                    {

                        providedServiceID = model.providedServiceID,
                        providedServiceDateTime = model.providedServiceDateTime,
                        providedServiceStatus = model.providedServiceStatus,
                        clientID = model.clientID,
                        masterID = model.masterID,
                        serviceName = model.serviceName
                    };

                    try
                    {
                        db.providedServices.Attach(editedService);
                        db.Entry(editedService).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                    
                    catch(Exception e)
                    {
                        ModelState.AddModelError("", e.Message);
                    }


                    if (ModelState.IsValid)
                    {
                        return RedirectToAction("ListOfProvidedServices");
                    }
                }

            }
            using (var db = new Entities())
            {
                var cl = db.Clients.ToList();
                var ms = db.Masters.ToList();
                var sv = db.Services.ToList();

                model.ClientsList = cl.Select(c => new SelectListItem
                {
                    Value = c.clientID.ToString(),
                    Text = $"ФИО: {c.surname} {c.name} {c.patronymic}   Номер телефона:{c.phoneNumber}"
                }).ToList();

                model.MastersList = ms.Select(m => new SelectListItem
                {
                    Value = m.masterID.ToString(),
                    Text = $"ФИО: {m.surname} {m.name} {m.patronymic}"
                }).ToList();


                model.ServicesList = sv.Select(s => new SelectListItem
                {
                    Value = s.serviceName.ToString(),
                    Text = $"Название услуги: {s.serviceName}"
                }).ToList();

            }


            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Master")]

        public ActionResult DeleteProvidedService(Guid providedServiceID)
        {
            providedService psToDelete;
            var db = new Entities();
            psToDelete = db.providedServices.Find(providedServiceID);


            return View(psToDelete);
        }

        [HttpPost,ActionName("DeleteProvidedService")]
        public ActionResult DeleteProvidedServiceConfirmed(Guid providedServiceID)
        {
            var db = new Entities();

            providedService psToDelete = db.providedServices.Find(providedServiceID);
            db.providedServices.Remove(psToDelete);
            db.SaveChanges();
            return RedirectToAction("ListOfProvidedServices");
        }




        //работа с мастерами


        [HttpGet]
        [AllowAnonymous]
        [Authorize(Roles = "Master,Client")]
        public ActionResult ListOfMasters()
        {
            List<Master> master = new List<Master>();
            using (var db = new Entities())
            {
                master = db.Masters.OrderByDescending(x => x.surname)
                                                .ThenBy(x => x.name)
                                                .ThenBy(x => x.patronymic).
                                                ToList();
            }
            return View(master);
        }


        [HttpGet]
        [Authorize(Roles = "Master")]
        public ActionResult MasterDetails(Guid masterID)
        {
            using (var db = new Entities())
            {
                // Найти мастера по ID
                var master = db.Masters.Find(masterID);
                if (master == null)
                    return HttpNotFound();

                // Получить все услуги, связанные с этим мастером
                var services = db.Masters_Services
                    .Where(ms => ms.masterID == masterID)
                    .Select(ms => new ServiceVM
                    {
                        ServiceName = ms.serviceName,
                        ExecutionTime = ms.executionTime
                    })
                    .ToList();

                // Создать модель представления
                var model = new MasterDetailsVM
                {
                    Master = master,
                    Services = services
                };

                return View(model);
            }
        }




      






        [HttpGet]
        [Authorize(Roles = "Master")]
        public ActionResult CreateMaster()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateMaster(MasterVM model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new Entities())
                {
                    Master master = new Master
                    {
                        masterID = Guid.NewGuid(),
                        surname = model.surname,
                        name = model.name,
                        patronymic = model.patronymic,
                        
                    };
                    db.Masters.Add(master);
                    db.SaveChanges();
                    return RedirectToAction("ListOfMasters");
                }
            }

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Master")]
        public ActionResult EditMaster(Guid masterID)
        {
            MasterVM model;
            using (var db = new Entities())
            {
                Master m = db.Masters.Find(masterID);
                if (m == null) return HttpNotFound(); // Проверка на наличие мастера в базе

                model = new MasterVM
                {
                    masterID = m.masterID,
                    name = m.name,
                    surname = m.surname,
                    patronymic = m.patronymic,
                    // Добавьте другие необходимые поля
                };
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditMaster(MasterVM model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new Entities())
                {
                    Master editedMaster = new Master
                    {
                        masterID = model.masterID,
                        name = model.name,
                        surname = model.surname,
                        patronymic = model.patronymic,
                        // Добавьте другие необходимые поля
                    };

                    db.Masters.Attach(editedMaster);
                    db.Entry(editedMaster).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                return RedirectToAction("ListOfMasters");
            }
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Master")]
        public ActionResult DeleteMaster(Guid masterID)
        {
            Master masterToDelete;
            using (var db = new Entities())
            {
                masterToDelete = db.Masters.Find(masterID);
                if (masterToDelete == null)
                    return HttpNotFound(); // Проверка на наличие мастера в базе
               
            }
            return View(masterToDelete);
        }

        [HttpPost, ActionName("DeleteMaster")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteMasterConfirmed(Guid masterID)
        {
            using (var db = new Entities())
            {
                Master masterToDelete = db.Masters.Find(masterID);
                if (masterToDelete == null) return HttpNotFound(); // Проверка на наличие мастера в базе

                // Удаление связанных с данным мастером записей, если это необходимо
                //db.Masters_Services.RemoveRange(masterToDelete.Masters_Services);
                db.Masters.Remove(masterToDelete);
                db.SaveChanges();
            }

            return RedirectToAction("ListOfMasters");
        }


        //работа с Masters-Services


        [HttpGet]
        [AllowAnonymous]
        [Authorize(Roles = "Master")]
        public ActionResult ListOfMS()
        {
            List<Masters_Services> ms = new List<Masters_Services>();
            var db = new Entities();
            
             ms = db.Masters_Services.OrderByDescending(x => x.masterID)
                                                .ThenBy(x => x.serviceName)
                                                .ThenBy(x => x.executionTime).
                                                ToList();
            
            return View(ms);
        }


        [HttpGet]
        [Authorize(Roles = "Master")]
        public ActionResult CreateMS()
        {
            using (var db = new Entities())
            {
                var masters = db.Masters.ToList().Select(m => new SelectListItem
                {
                    Value = m.masterID.ToString(),
                    Text = $"ФИО: {m.surname} {m.name} {m.patronymic}"
                }).ToList();

                var services = db.Services.ToList().Select(s => new SelectListItem
                {
                    Value = s.serviceName,
                    Text = s.serviceName
                }).ToList();

                var viewModel = new MSVM
                {
                    MastersList = masters,
                    ServicesList = services,
                };

                return View(viewModel);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateMS(MSVM viewModel)
        {
            if (ModelState.IsValid)
            {
                using (var db = new Entities())
                {
                    // Проверяем, оказывает ли уже данный мастер указанную услугу
                    bool alreadyExists = db.Masters_Services.Any(ms => ms.masterID == viewModel.MasterID && ms.serviceName == viewModel.ServiceName);

                    if (alreadyExists)
                    {
                        // Если да, то возвращаем ошибку
                        ModelState.AddModelError("", "Указанный мастер уже оказывает данную услугу.");
                        // Перезаполним данные для выпадающих списков
                        PopulateDropDownLists(viewModel, db);
                        // Отправляем модель обратно в представление
                        return View(viewModel);
                    }

                    // Создаем новый экземпляр Masters_Services и заполняем его данными из ViewModel
                    Masters_Services newMasterService = new Masters_Services
                    {
                        masterID = viewModel.MasterID,
                        serviceName = viewModel.ServiceName,
                        executionTime = viewModel.ExecutionTime
                    };

                    // Добавляем в базу данных
                    db.Masters_Services.Add(newMasterService);
                    db.SaveChanges();

                    // Перенаправляем на список услуг, предоставленных мастерами
                    return RedirectToAction("ListOfMS");
                }
            }

            // Если модель не валидна, возвращаем её обратно в представление с ошибками
            using (var db = new Entities())
            {
                PopulateDropDownLists(viewModel, db);
            }

            ModelState.AddModelError("", "Проверьте введенные данные");
            return View(viewModel);
        }

        private void PopulateDropDownLists(MSVM viewModel, Entities db)
        {
            viewModel.MastersList = db.Masters.ToList().Select(m => new SelectListItem
            {
                Value = m.masterID.ToString(),
                Text = $"{m.surname} {m.name} {m.patronymic}"
            }).ToList();

            viewModel.ServicesList = db.Services.ToList().Select(s => new SelectListItem
            {
                Value = s.serviceName,
                Text = s.serviceName
            }).ToList();
        }





        [HttpGet]
        [Authorize(Roles = "Master")]
        public ActionResult EditMS(Guid masterId, string serviceName)
        {
            MSVM model;
            using (var db = new Entities())
            {
                Masters_Services masterService = db.Masters_Services
                    .FirstOrDefault(ms => ms.masterID == masterId && ms.serviceName == serviceName);
                if (masterService == null)
                {
                    return HttpNotFound();
                }

                model = new MSVM
                {
                    ExecutionTime = masterService.executionTime,
                    MasterID = masterService.masterID,
                    ServiceName = masterService.serviceName,
                    MastersList = db.Masters.ToList().Select(m => new SelectListItem
                    {
                        Value = m.masterID.ToString(),
                        Text = $"ФИО: {m.surname} {m.name} {m.patronymic}"
                    }).ToList(),
                    ServicesList = db.Services.ToList().Select(s => new SelectListItem
                    {
                        Value = s.serviceName,
                        Text = $"Название услуги: {s.serviceName}" // предположение, что "name" - название услуги
                    }).ToList()
                };
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditMS(MSVM model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new Entities())
                {
                    Masters_Services masterService = db.Masters_Services
                        .FirstOrDefault(ms => ms.masterID == model.MasterID && ms.serviceName == model.ServiceName);

                    if (masterService == null)
                    {
                        ModelState.AddModelError("", "Услуга мастера не найдена.");
                    }
                    else
                    {
                        masterService.executionTime = model.ExecutionTime;
                        // Обновляем только время выполнения, т.к. MasterID и ServiceName являются ключами

                        try
                        {
                            db.Entry(masterService).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            return RedirectToAction("ListOfMS"); // Предположим, что это метод, возвращающий список услуг
                        }
                        catch (Exception e)
                        {
                            ModelState.AddModelError("", e.Message);
                        }
                    }
                }
            }

            // Повторно загружаем списки мастеров и услуг для модели, если сохранение завершилось с ошибкой
            using (var db = new Entities())
            {
                model.MastersList = db.Masters.ToList().Select(m => new SelectListItem
                {
                    Value = m.masterID.ToString(),
                    Text = $"ФИО: {m.surname} {m.name} {m.patronymic}"
                }).ToList();

                model.ServicesList = db.Services.ToList().Select(s => new SelectListItem
                {
                    Value = s.serviceName,
                    Text = $"Название услуги: {s.serviceName}" // предположение, что "name" - название услуги
                }).ToList();
            }
            return View(model);
        }


        [HttpGet]
        [Authorize(Roles = "Master")]
        public ActionResult DeleteMS(Guid masterId, string serviceName)
        {
            Masters_Services msToDelete;
           var db = new Entities();
            
                msToDelete = db.Masters_Services.Find(masterId, serviceName);
                if (msToDelete == null)
                    return HttpNotFound(); // Проверка на наличие мастера в базе

            
            return View(msToDelete);
        }



        //для удаление всех записей надо включить каскадное удаление, поскольку некоторые записи не могут быть удалены, т.к используются в других таблицах
        [HttpPost, ActionName("DeleteMS")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteMSConfirmed(Guid masterId, string serviceName)
        {
            var db = new Entities();
            
                Masters_Services msToDelete = db.Masters_Services.Find(masterId, serviceName);
                if (msToDelete == null) return HttpNotFound(); // Проверка на наличие мастера в базе

                // Удаление связанных с данным мастером записей, если это необходимо
                //db.Masters_Services.RemoveRange(masterToDelete.Masters_Services);
                db.Masters_Services.Remove(msToDelete);
                db.SaveChanges();
            

            return RedirectToAction("ListOfMS");
        }






        string ReturnHashCode(string loginAndSalt)
        {
            string hash = "";
            using (SHA1 sha1Hash = SHA1.Create())
            {
                byte[] data = sha1Hash.ComputeHash(Encoding.UTF8.GetBytes(loginAndSalt));
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                hash = sBuilder.ToString().ToUpper();
            }
            return hash;
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(UserVM webUser)
        {
            if (ModelState.IsValid)
            {
                using (Entities context = new Entities())
                {
                    //идентификация
                    User user = null;
                    user = context.Users.Where(u => u.Login == webUser.Login).FirstOrDefault();
                    if (user != null)
                    {
                        //аутентификация
                        string passwordHash = ReturnHashCode(webUser.Password + user.Salt.ToString().ToUpper());
                        if (passwordHash == user.PasswordHash)
                        {
                            string userRole = "";

                            switch (user.UserRole)
                            {
                                case 1:
                                    userRole = "Master";
                                    break;

                                case 2:
                                    userRole = "Client";
                                    break;
                                

                            }



                            FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(1, user.Login, DateTime.Now, DateTime.Now.AddDays(1), false, userRole);
                            string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                            HttpContext.Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket));

                            return RedirectToAction("ListOfClients", "main");

                        }
                    }
                }
            }


            ViewBag.Error = "Пользователя с таким логином и паролем не существует, попробуйте еще";
            return View(webUser);
        }


        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("ListOfClients", "main");
        }


    }
}
