import { HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

//#region ODataApiGen Imports
import {
  Model,
  ModelField,
  ODataModel,
  ODataCollection,
  ODataOptions,
  ODataQueryArgumentsOptions,
  Duration,
} from 'angular-odata';//#endregion

//#region ODataApiGen Imports
{% if HasGeoFields %}import { {% for p in GeoFields %}{{p.Type}}{% unless forloop.last %},{% endunless %}{% endfor %} } from 'geojson';
{% endif %}{% for import in Imports %}import { {{import.Names | join: ", "}} } from '{{import.Path}}';
{% endfor %}//#endregion

@Model()
export class {{Name}}<E extends {{Entity.Name}}> extends {% if Base != null %}{{Base.Name}}<E>{% else %}ODataModel<E>{% endif %} {
  //#region ODataApiGen Properties
  {% for field in Fields %}@ModelField()
  {{field.Name}}: {{field.Type}};
  {{field.Resource}}
  {{field.Getter}}
  {{field.Setter}}
  {% endfor %}//#endregion
  //#region ODataApiGen Actions
  {% for action in Actions %}{{action}}
  {% endfor %}//#endregion
  //#region ODataApiGen Functions
  {% for func in Functions %}{{func}}
  {% endfor %}//#endregion
  //#region ODataApiGen Navigations
  {% for nav in Navigations %}{{nav}}
  {% endfor %}//#endregion
}