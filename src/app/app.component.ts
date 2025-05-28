import { Component, Inject, OnInit, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import Map from 'ol/Map';
import View from 'ol/View';
import TileLayer from 'ol/layer/Tile';
import OSM from 'ol/source/OSM';
import VectorLayer from 'ol/layer/Vector';
import VectorSource from 'ol/source/Vector';
import Draw from 'ol/interaction/Draw';
import { Fill, Stroke, Style, Text } from 'ol/style';
import { FeatureLike } from 'ol/Feature';
import { Geometry } from 'ol/geom';
import { DrawingService } from './drawing.service';
import GeoJSON from 'ol/format/GeoJSON';
import Select from 'ol/interaction/Select';
import { click } from 'ol/events/condition';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  map!: Map;
  draw!: Draw;
  selectedFeature: any = null;
  vectorSource = new VectorSource();
  vectorLayer!: VectorLayer<VectorSource>;
  selectInteraction!: Select;

  constructor(
    @Inject(PLATFORM_ID) private platformId: Object,
    private drawingService: DrawingService
  ) {}

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.vectorLayer = new VectorLayer({
        source: this.vectorSource,
        style: (feature: FeatureLike) => {
          const name = feature.get('name') || '';
          return new Style({
            fill: new Fill({ color: 'rgba(0, 255, 255, 0.2)' }),
            stroke: new Stroke({ color: '#00cccc', width: 2 }),
            text: new Text({
              text: name,
              font: '14px Calibri,sans-serif',
              fill: new Fill({ color: '#000' }),
              stroke: new Stroke({ color: '#fff', width: 2 })
            })
          });
        }
      });

      this.map = new Map({
        target: 'map',
        layers: [new TileLayer({ source: new OSM() }), this.vectorLayer],
        view: new View({
          center: [0, 0],
          zoom: 2,
          projection: 'EPSG:3857'
        })
      });

      this.loadSavedDrawings();
      this.addSelectInteraction();
    }
  }

  startDrawing(type: 'Polygon' | 'Circle'): void {
    if (this.draw) {
      this.map.removeInteraction(this.draw);
    }

    this.draw = new Draw({
      source: this.vectorSource,
      type: type
    });

    this.draw.on('drawend', (event) => {
      const feature = event.feature;
      const name = prompt('Bu şekle bir isim verin:');
      if (name) {
        feature.set('name', name);
        const geojson = new GeoJSON().writeFeatureObject(feature);
        this.drawingService.saveDrawing({ name, geoJson: JSON.stringify(geojson) }).subscribe();
        this.vectorLayer.changed();
      }
    });

    this.map.addInteraction(this.draw);
  }

  loadSavedDrawings(): void {
    this.drawingService.getAllDrawings().subscribe(drawings => {
      const format = new GeoJSON();
      drawings.forEach(drawing => {
        const feature = format.readFeature(drawing.geoJson, {
          featureProjection: 'EPSG:3857'
        });
        if (Array.isArray(feature)) {
          feature.forEach(f => {
            f.set('name', drawing.name);
            f.set('id', drawing.id);
            this.vectorSource.addFeature(f);
          });
        } else {
          feature.set('name', drawing.name);
          feature.set('id', drawing.id);
          this.vectorSource.addFeature(feature);
        }
      });
    });
  }

  addSelectInteraction(): void {
    this.selectInteraction = new Select({ condition: click });
    this.map.addInteraction(this.selectInteraction);

    this.selectInteraction.on('select', (e) => {
      this.selectedFeature = e.selected[0] || null;
    });
  }

  deleteSelectedFeature(): void {
    if (!this.selectedFeature) {
      alert('Önce silmek için bir şekil seçin!');
      return;
    }

    const id = this.selectedFeature.get('id');
    if (!id) {
      alert('Silinemez: Bu şekil veritabanında kayıtlı değil.');
      return;
    }

    this.drawingService.deleteDrawing(id).subscribe(() => {
      this.vectorSource.removeFeature(this.selectedFeature);
      alert('Şekil silindi!');
      this.selectedFeature = null;
    }, error => {
      alert('Silme işlemi başarısız: ' + error.message);
    });
  }

  updateSelectedFeature(): void {
    if (!this.selectedFeature) {
      alert('Lütfen önce bir şekil seçin.');
      return;
    }

    const newName = prompt('Yeni isim girin:', this.selectedFeature.get('name'));
    if (!newName) {
      return;
    }

    this.selectedFeature.set('name', newName);
    const updatedGeoJson = new GeoJSON().writeFeatureObject(this.selectedFeature);
    const id = this.selectedFeature.get('id');

    this.drawingService.updateDrawing(id, {
      id,
      name: newName,
      geoJson: JSON.stringify(updatedGeoJson)
    }).subscribe(() => {
      alert('İsim güncellendi!');
      this.vectorLayer.changed();
    }, error => {
      alert('Güncelleme başarısız: ' + error.message);
    });
  }
}
