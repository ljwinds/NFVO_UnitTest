using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NFVO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace UnitTest
{
    public class KeystoneAPI
    {
        public String GetToken()
        {
            String method = "POST";
            String endPoint = "http://10.109.252.111:5000/v3/auth/tokens";
            String contentType = "application/json";
            String postData = "{\"auth\": {\"identity\": {\"methods\": [\"password\"],\"password\": {\"user\": {\"name\": \"admin\",\"domain\": {\"name\": \"default\"},\"password\": \"123456\"}}},\"scope\": {\"project\": {\"id\": \"63f60f6af268419d87b4633f8b4094c4\"}}}}";

            RestClient restClient = new RestClient(method, endPoint, contentType);
            HttpWebResponse response = restClient.MakeRequest("", postData, "");

            return response.GetResponseHeader("X-Subject-Token");
        }
    }

    public class NeutronAPI
    {
        public String Token { get; set; }

        public NeutronAPI()
        {
            KeystoneAPI k = new KeystoneAPI();
            this.Token = k.GetToken();
        }

        // 获取当前所有的网络
        public List<NetworkModels> ListNetworks()
        {
            String method = "GET";
            String endPoint = "http://10.109.252.111:9696/v2.0/networks";
            String contentType = "application/json";

            RestClient restClient = new RestClient(method, endPoint, contentType);
            HttpWebResponse response = restClient.MakeRequest("", "", Token);

            using (System.IO.StreamReader reader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.GetEncoding("utf-8")))
            {
                String value = reader.ReadToEnd();
                JObject obj = (JObject)JsonConvert.DeserializeObject(value);

                List<NetworkModels> networks = new List<NetworkModels>();

                foreach (var network in obj["networks"])
                {
                    NetworkModels temp = new NetworkModels();
                    temp.ProviderPhysicalNetwork = network["provider:physical_network"].ToString();
                    temp.ID = network["id"].ToString();
                    temp.ProviderNetworkType = network["provider:network_type"].ToString();
                    temp.ProjectID = network["project_id"].ToString();
                    temp.RouterExternal = network["shared"].ToString();
                    temp.Shared = network["router:external"].ToString();
                    temp.Name = network["name"].ToString();

                    temp.Subnets = new List<String>();
                    foreach (var subnet in network["subnets"])
                    {
                        temp.Subnets.Add(subnet.ToString());
                    }

                    networks.Add(temp);
                }

                return networks;
            }
        }

        // 获取特定subnet_id的子网
        public SubnetModels GetSubnet(String subnet_id)
        {
            String method = "GET";
            String endPoint = "http://10.109.252.111:9696/v2.0/subnets/";
            String contentType = "application/json";

            RestClient restClient = new RestClient(method, endPoint, contentType);
            HttpWebResponse response = restClient.MakeRequest(subnet_id, "", Token);

            using (System.IO.StreamReader reader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.GetEncoding("utf-8")))
            {
                String value = reader.ReadToEnd();
                JObject obj = (JObject)JsonConvert.DeserializeObject(value);

                SubnetModels subnet = new SubnetModels();

                subnet.ID = obj["subnet"]["id"].ToString();
                subnet.NetworkID = obj["subnet"]["network_id"].ToString();
                subnet.Name = obj["subnet"]["name"].ToString();
                subnet.ProjectID = obj["subnet"]["project_id"].ToString();
                subnet.CIDR = obj["subnet"]["cidr"].ToString();
                subnet.GatewayIP = obj["subnet"]["gateway_ip"].ToString();

                return subnet;
            }
        }

        // 获取当前所有的路由器
        public List<RouterModels> ListRouters()
        {
            String method = "GET";
            String endPoint = "http://10.109.252.111:9696/v2.0/routers";
            String contentType = "application/json";

            RestClient restClient = new RestClient(method, endPoint, contentType);
            HttpWebResponse response = restClient.MakeRequest("", "", Token);

            using (System.IO.StreamReader reader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.GetEncoding("utf-8")))
            {
                String value = reader.ReadToEnd();
                JObject obj = (JObject)JsonConvert.DeserializeObject(value);

                List<RouterModels> routers = new List<RouterModels>();

                foreach (var router in obj["routers"])
                {
                    RouterModels temp = new RouterModels();

                    temp.ProjectID = router["project_id"].ToString();
                    temp.ID = router["id"].ToString();
                    temp.Name = router["name"].ToString();

                    temp.ExternalGatewayInfo = new GatewayInfoModels();
                    temp.ExternalGatewayInfo.EnableSNAT = router["external_gateway_info"]["enable_snat"].ToString();
                    temp.ExternalGatewayInfo.NetworkID = router["external_gateway_info"]["network_id"].ToString();

                    routers.Add(temp);
                }

                return routers;
            }
        }

        // 获取特定device_id的接口
        public List<PortModels> ListRouterPorts(String device_id)
        {
            String method = "GET";
            String endPoint = "http://10.109.252.111:9696/v2.0/ports?device_id=" + device_id;
            String contentType = "application/json";

            RestClient restClient = new RestClient(method, endPoint, contentType);
            HttpWebResponse response = restClient.MakeRequest("", "", Token);

            using (System.IO.StreamReader reader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.GetEncoding("utf-8")))
            {
                String value = reader.ReadToEnd();
                JObject obj = (JObject)JsonConvert.DeserializeObject(value);

                List<PortModels> ports = new List<PortModels>();

                foreach (var port in obj["ports"])
                {
                    PortModels temp = new PortModels();

                    temp.DeviceID = port["device_id"].ToString();
                    temp.ProjectID = port["project_id"].ToString();
                    temp.MacAddress = port["mac_address"].ToString();
                    temp.NetworkID = port["network_id"].ToString();

                    temp.FixedIPs = new List<FixedIPModels>();
                    foreach (var fixed_ip in port["fixed_ips"])
                    {
                        FixedIPModels fip = new FixedIPModels();
                        fip.SubnetID = fixed_ip["subnet_id"].ToString();
                        fip.IPAddress = fixed_ip["ip_address"].ToString();

                        temp.FixedIPs.Add(fip);
                    }

                    ports.Add(temp);
                }

                return ports;
            }
        }

    }

    public class TackerAPI
    {
        public String Token { get; set; }

        public TackerAPI()
        {
            KeystoneAPI k = new KeystoneAPI();
            this.Token = k.GetToken();
        }


        public VimModels RegisterVim()
        {
            String method = "POST";
            String endPoint = "http://10.109.252.111:9890/v1.0/vims";
            String contentType = "application/json";

            // Http post body.
            JObject requestVim = new JObject();
            requestVim["vim"] = new JObject();
            requestVim["vim"]["tenant_id"] = "63f60f6af268419d87b4633f8b4094c4";
            requestVim["vim"]["type"] = "openstack";
            requestVim["vim"]["auth_url"] = "http://10.109.252.111:5000/v3/";
            requestVim["vim"]["auth_cred"] = new JObject();
            requestVim["vim"]["auth_cred"]["username"] = "tacker";
            requestVim["vim"]["auth_cred"]["user_domain_name"] = "Default";
            requestVim["vim"]["auth_cred"]["password"] = "123456";
            requestVim["vim"]["vim_project"] = new JObject();
            requestVim["vim"]["vim_project"]["name"] = "service";
            requestVim["vim"]["vim_project"]["project_domain_name"] = "Default";
            requestVim["vim"]["name"] = "VIM2";


            VimModels RequestVim = new VimModels();
            // RequestVim.TenantId = "63f60f6af268419d87b4633f8b4094c4";
            RequestVim.Type = "openstack";
            RequestVim.AuthUrl = "http://10.109.252.111:5000/v3/";

            RequestVim.AuthCred = new VimAuthCredModels();
            RequestVim.AuthCred.UserName = "tacker";
            RequestVim.AuthCred.UserDomainName = "Default";
            RequestVim.AuthCred.Password = "123456";

            RequestVim.VimProject = new VimProjectModels();
            RequestVim.VimProject.Name = "service";
            RequestVim.VimProject.ProjectDomainName = "Default";

            RequestVim.Name = "VIM2";

            String postData = JsonConvert.SerializeObject(requestVim);
            RestClient restClient = new RestClient(method, endPoint, contentType);
            HttpWebResponse response = restClient.MakeRequest("", postData, Token);

            using (System.IO.StreamReader reader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.GetEncoding("utf-8")))
            {
                String value = reader.ReadToEnd();
                JObject obj = (JObject)JsonConvert.DeserializeObject(value);

                /* List<PortModels> ports = new List<PortModels>();

                foreach (var port in obj["ports"])
                {
                    PortModels temp = new PortModels();

                    temp.DeviceID = port["device_id"].ToString();
                    temp.ProjectID = port["project_id"].ToString();
                    temp.MacAddress = port["mac_address"].ToString();
                    temp.NetworkID = port["network_id"].ToString();

                    temp.FixedIPs = new List<FixedIPModels>();
                    foreach (var fixed_ip in port["fixed_ips"])
                    {
                        FixedIPModels fip = new FixedIPModels();
                        fip.SubnetID = fixed_ip["subnet_id"].ToString();
                        fip.IPAddress = fixed_ip["ip_address"].ToString();

                        temp.FixedIPs.Add(fip);
                    }

                    ports.Add(temp);
                }*/

                //return ports;
                return RequestVim;
            }

        }

        public void CreateVnfd()
        {
            String method = "POST";
            String endPoint = "http://10.109.252.111:9890/v1.0/vnfds";
            String contentType = "application/json";

            // Http post body.
            JObject requestVnfd = new JObject();
            requestVnfd["vnfd"] = new JObject();
            requestVnfd["vnfd"]["tenant_id"] = "63f60f6af268419d87b4633f8b4094c4";
            requestVnfd["vnfd"]["name"] = "vnfd2";

            JObject tempObj = new JObject();
            tempObj["service_type"] = "vnfd";
            requestVnfd["vnfd"]["service_types"] = new JArray() { tempObj };
            requestVnfd["vnfd"]["attributes"] = new JObject();
            requestVnfd["vnfd"]["attributes"]["vnfd"] = new JObject();
            requestVnfd["vnfd"]["attributes"]["vnfd"]["tosca_definitions_version"] = "tosca_simple_profile_for_nfv_1_0_0";
            requestVnfd["vnfd"]["attributes"]["vnfd"]["description"] = "Demo example";
            requestVnfd["vnfd"]["attributes"]["vnfd"]["metadata"] = new JObject();
            requestVnfd["vnfd"]["attributes"]["vnfd"]["metadata"]["template_name"] = "sample-tosca-vnfd";
            requestVnfd["vnfd"]["attributes"]["vnfd"]["topology_template"] = new JObject();
            requestVnfd["vnfd"]["attributes"]["vnfd"]["topology_template"]["node_templates"] = new JObject();
            requestVnfd["vnfd"]["attributes"]["vnfd"]["topology_template"]["node_templates"]["VDU1"] = new JObject();
            requestVnfd["vnfd"]["attributes"]["vnfd"]["topology_template"]["node_templates"]["VDU1"]["type"] = "tosca.nodes.nfv.VDU.Tacker";
            requestVnfd["vnfd"]["attributes"]["vnfd"]["topology_template"]["node_templates"]["VDU1"]["capabilities"] = new JObject();
            requestVnfd["vnfd"]["attributes"]["vnfd"]["topology_template"]["node_templates"]["VDU1"]["capabilities"]["nfv_compute"] = new JObject();
            requestVnfd["vnfd"]["attributes"]["vnfd"]["topology_template"]["node_templates"]["VDU1"]["capabilities"]["nfv_compute"]["properties"] = new JObject();
            requestVnfd["vnfd"]["attributes"]["vnfd"]["topology_template"]["node_templates"]["VDU1"]["capabilities"]["nfv_compute"]["properties"]["num_cpus"] = 1;
            requestVnfd["vnfd"]["attributes"]["vnfd"]["topology_template"]["node_templates"]["VDU1"]["capabilities"]["nfv_compute"]["properties"]["mem_size"] = "512 MB";
            requestVnfd["vnfd"]["attributes"]["vnfd"]["topology_template"]["node_templates"]["VDU1"]["capabilities"]["nfv_compute"]["properties"]["disk_size"] = "1 GB";
            requestVnfd["vnfd"]["attributes"]["vnfd"]["topology_template"]["node_templates"]["VDU1"]["properties"] = new JObject();
            requestVnfd["vnfd"]["attributes"]["vnfd"]["topology_template"]["node_templates"]["VDU1"]["properties"]["image"] = "ubuntu16.04";
            requestVnfd["vnfd"]["attributes"]["vnfd"]["topology_template"]["node_templates"]["CP1"] = new JObject();
            requestVnfd["vnfd"]["attributes"]["vnfd"]["topology_template"]["node_templates"]["CP1"]["type"] = "tosca.nodes.nfv.CP.Tacker";
            requestVnfd["vnfd"]["attributes"]["vnfd"]["topology_template"]["node_templates"]["CP1"]["properties"] = new JObject();
            requestVnfd["vnfd"]["attributes"]["vnfd"]["topology_template"]["node_templates"]["CP1"]["properties"]["order"] = 0;
            requestVnfd["vnfd"]["attributes"]["vnfd"]["topology_template"]["node_templates"]["CP1"]["properties"]["management"] = true;
            requestVnfd["vnfd"]["attributes"]["vnfd"]["topology_template"]["node_templates"]["CP1"]["properties"]["anti_spoofing_protection"] = false;

            JObject tempObj1 = new JObject();
            tempObj1["virtualLink"] = new JObject();
            tempObj1["virtualLink"]["node"] = "VL1";

            JObject tempObj2 = new JObject();
            tempObj2["virtualBinding"] = new JObject();
            tempObj2["virtualBinding"]["node"] = "VDU1";

            requestVnfd["vnfd"]["attributes"]["vnfd"]["topology_template"]["node_templates"]["CP1"]["requirements"] = new JArray() { tempObj1, tempObj2 };
            requestVnfd["vnfd"]["attributes"]["vnfd"]["topology_template"]["node_templates"]["VL1"] = new JObject();
            requestVnfd["vnfd"]["attributes"]["vnfd"]["topology_template"]["node_templates"]["VL1"]["type"] = "tosca.nodes.nfv.VL";
            requestVnfd["vnfd"]["attributes"]["vnfd"]["topology_template"]["node_templates"]["VL1"]["properties"] = new JObject();
            requestVnfd["vnfd"]["attributes"]["vnfd"]["topology_template"]["node_templates"]["VL1"]["properties"]["vendor"] = "tacker";
            requestVnfd["vnfd"]["attributes"]["vnfd"]["topology_template"]["node_templates"]["VL1"]["properties"]["network_name"] = "green";

            String postData = JsonConvert.SerializeObject(requestVnfd);
            RestClient restClient = new RestClient(method, endPoint, contentType);
            HttpWebResponse response = restClient.MakeRequest("", postData, Token);

            using (System.IO.StreamReader reader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.GetEncoding("utf-8")))
            {
                String value = reader.ReadToEnd();
                JObject obj = (JObject)JsonConvert.DeserializeObject(value);
            }
        }
    }
}